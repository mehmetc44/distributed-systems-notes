# Nihai Tutarlılık (Saga Choreography) Mimarisi ve İletişim Akış Kılavuzu

Bu doküman, e-ticaret sipariş sürecinde **Nihai Tutarlılık (Eventual Consistency)** sağlamak amacıyla uygulanan **Saga Choreography (Koreografi)** deseninin mimarisini ve servisler arası event akışını detaylandırmaktadır.

---

# Genel Mimari

Saga Koreografi yaklaşımında merkezi bir yönlendirici (Orchestrator) bulunmaz. Her mikroservis kendi sorumluluğundaki işlemi tamamlar ve bir sonraki adımı tetikleyecek **Event (Olay)** yayınlar. Bir hata durumunda ise **Telafi Edici İşlemler (Compensating Transactions)** çalıştırılarak sistem nihai tutarlılığa ulaştırılır.

Projedeki bileşenler:

- **Order.API** (SQLite DB)
  - Siparişi `Suspend` durumunda başlatır.
  - Ödeme ve stok sonuçlarına göre sipariş durumunu `Completed` veya `Fail` olarak günceller.

- **Stock.API** (MongoDB)
  - Stok bilgilerini MongoDB üzerinde yönetir.
  - `OrderCreatedEvent` geldiğinde stok kontrolü ve rezervasyonu yapar.
  - Ödeme başarısız olduğunda (`PaymentFailedEvent`) stokları geri yükleyerek **telafi edici işlemi** gerçekleştirir.

- **Payment.API**
  - `StockReservedEvent` geldiğinde ödeme alma simülasyonunu çalıştırır.
  - İşlem sonucuna göre `PaymentCompletedEvent` veya `PaymentFailedEvent` yayınlar.

- **Shared & RabbitMQ**
  - Tüm servisler MassTransit ve RabbitMQ üzerinden ortak kontratlar (`Shared`) vasıtasıyla asenkron haberleşir.

---

# Saga Koreografi Akış Diyagramı

```mermaid
sequenceDiagram
    participant Client as Müşteri
    participant OrderAPI as Order.API (SQLite)
    participant RabbitMQ
    participant StockAPI as Stock.API (MongoDB)
    participant PaymentAPI as Payment.API

    Client->>OrderAPI: POST /create-order
    OrderAPI->>OrderAPI: Sipariş Durumu = Suspend
    OrderAPI-->>RabbitMQ: OrderCreatedEvent

    RabbitMQ-->>StockAPI: OrderCreatedEvent
    StockAPI->>StockAPI: Stok Kontrolü (MongoDB)

    alt Yeterli Stok Var
        StockAPI->>StockAPI: Stok Miktarını Düşür
        StockAPI-->>RabbitMQ: StockReservedEvent

        RabbitMQ-->>PaymentAPI: StockReservedEvent
        PaymentAPI->>PaymentAPI: Ödeme İşlemi Yap

        alt Ödeme Başarılı
            PaymentAPI-->>RabbitMQ: PaymentCompletedEvent
            RabbitMQ-->>OrderAPI: PaymentCompletedEvent
            OrderAPI->>OrderAPI: Sipariş Durumu = Completed

        else Ödeme Başarısız
            PaymentAPI-->>RabbitMQ: PaymentFailedEvent

            par Sipariş İptali
                RabbitMQ-->>OrderAPI: PaymentFailedEvent
                OrderAPI->>OrderAPI: Sipariş Durumu = Fail
            and Stok Telafisi (Compensating Transaction)
                RabbitMQ-->>StockAPI: PaymentFailedEvent
                StockAPI->>StockAPI: Rezerve Edilen Stokları Geri Ekle
            end
        end

    else Stok Yetersiz
        StockAPI-->>RabbitMQ: StockNotReservedEvent
        RabbitMQ-->>OrderAPI: StockNotReservedEvent
        OrderAPI->>OrderAPI: Sipariş Durumu = Fail
    end
```

---

# Olay (Event) & Consumer Matrisi

| Event | Yayınlayan (Publisher) | Tüketen (Consumer) | Yapılan İşlem / Telafi Mantığı |
|-------|------------------------|-------------------|--------------------------------|
| **OrderCreatedEvent** | `Order.API` | `Stock.API` | Sipariş oluşturuldu. Stok kontrolü yapılır. |
| **StockReservedEvent** | `Stock.API` | `Payment.API` | Stok ayrıldı. Ödeme çekme işlemi başlatılır. |
| **StockNotReservedEvent** | `Stock.API` | `Order.API` | Stok yetersiz. Sipariş durumu `Fail` yapılır. |
| **PaymentCompletedEvent** | `Payment.API` | `Order.API` | Ödeme alındı. Sipariş durumu `Completed` yapılır. |
| **PaymentFailedEvent** | `Payment.API` | `Order.API` & `Stock.API` | Ödeme alınamadı. Sipariş `Fail` yapılır ve **Stock.API stokları geri yükler (Compensating Transaction)**. |

---

# Proje Dizini Yapısı

```
3.EventualConsistencySagaChoreography/
├── Order.API/
│   ├── Consumers/ (PaymentCompletedConsumer, PaymentFailedEventConsumer, StockNotReservedEventConsumer)
│   └── Data/order.db
├── Stock.API/
│   ├── Consumers/ (OrderCreatedEventConsumer, PaymentFailedEventConsumer)
│   └── Services/MongoDBService.cs
├── Payment.API/
│   └── Consumers/ (StockReservedEventConsumer)
└── Shared/
    ├── Events/
    └── Messages/
```

---

# Kullanılan Tasarım Desenleri ve Teknolojiler

- **Eventual Consistency Model**
- **Saga Pattern (Choreography Variant)**
- **Compensating Transaction Pattern (Telafi Edici İşlem)**
- **MassTransit & RabbitMQ Message Broker**
- **Polyglot Persistence (SQLite & MongoDB)**
- **Shared Contract Library**

---

# Sonuç

Saga Koreografi deseni, merkezi bir bağımlılık (Single Point of Failure) oluşturmadan mikroservislerin event'ler üzerinden kendi kararlarını alarak çalışmasını sağlar. Hata durumlarında yayınlanan telafi event'leri (örneğin `PaymentFailedEvent` ile stokların iade edilmesi) sayesinde veri nihai olarak tutarlı (eventually consistent) kalır.
