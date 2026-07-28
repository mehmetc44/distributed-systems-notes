# Mikroservis İletişim Mimarisi ve Akış Kılavuzu

Bu doküman, e-ticaret sipariş sürecini simüle eden mikroservis projesinin iletişim mimarisini ve bileşenlerini açıklamaktadır. Proje, bir siparişin oluşturulmasından tamamlanmasına kadar olan süreci yönetmek için olay tabanlı (event-driven) bir mimari ve Saga Pattern kullanır.

## Genel Bakış

Proje 3 ana mikroservis, 1 paylaşımlı kütüphane ve bir mesajlaşma aracısından (Message Broker) oluşur:

-   **Order.API:** Siparişleri oluşturmaktan ve siparişin genel durumunu (başarılı, başarısız vb.) takip etmekten sorumludur.
-   **Stock.API:** Ürün stok durumunu yönetir. Gelen siparişlere göre stoğu ayırır.
-   **Payment.API:** Ödeme işlemlerini simüle eder. Stok ayrıldıktan sonra ödeme sürecini başlatır.
-   **Shared (Paylaşımlı Kütüphane):** Mikroservisler arasında ortak olan `Event` ve `Message` tanımlarını içerir. Bu sayede tüm servisler aynı kontrat üzerinden haberleşir.
-   **RabbitMQ (Message Broker):** Servisler arasındaki asenkron iletişimi sağlayan aracıdır. Olayları (Events) yayınlamak ve tüketmek (consume) için kullanılır.

---

## Mimari İletişim Şeması (Saga Akışı)

Aşağıdaki şema, bir sipariş oluşturulduğunda servisler arasında gerçekleşen olay akışını (Saga) göstermektedir.

```mermaid
sequenceDiagram
    participant Client as Müşteri
    participant Order.API
    participant RabbitMQ
    participant Stock.API
    participant Payment.API

    Client->>+Order.API: Yeni Sipariş Talebi (POST /api/orders)
    Order.API->>Order.API: Siparişi 'Askıda' (Pending) olarak kaydet
    Order.API-->>+RabbitMQ: **OrderCreatedEvent** yayınla
    deactivate Order.API
    
    RabbitMQ-->>+Stock.API: OrderCreatedEvent'i tüket
    Stock.API->>Stock.API: Stok kontrolü yap
    alt Stok Mevcut
        Stock.API->>Stock.API: Stoğu rezerve et/azalt
        Stock.API-->>+RabbitMQ: **StockReservedEvent** yayınla
    else Stok Mevcut Değil
        Stock.API-->>+RabbitMQ: **StockNotReservedEvent** yayınla
    end
    deactivate Stock.API

    alt Stok Rezerve Edildi
        RabbitMQ-->>+Payment.API: StockReservedEvent'i tüket
        Payment.API->>Payment.API: Ödeme işlemini gerçekleştir
        alt Ödeme Başarılı
            Payment.API-->>+RabbitMQ: **PaymentCompletedEvent** yayınla
        else Ödeme Başarısız
            Payment.API-->>+RabbitMQ: **PaymentFailedEvent** yayınla
        end
        deactivate Payment.API
    end

    alt Nihai Durum Güncellemesi
        
        subDiagram Ödeme Başarılı Akışı
            RabbitMQ-->>+Order.API: PaymentCompletedEvent'i tüket
            Order.API->>Order.API: Sipariş durumunu 'Tamamlandı' (Completed) olarak güncelle
            deactivate Order.API
        end

        subDiagram Ödeme Başarısız Akışı
             RabbitMQ-->>+Order.API: PaymentFailedEvent'i tüket
             Order.API->>Order.API: Sipariş durumunu 'Başarısız' (Failed) olarak güncelle
             Note right of Order.API: Telafi edici işlem: Stok serbest bırakılmalı (Bu projede simüle edilmemiştir)
             deactivate Order.API
        end

        subDiagram Stok Yetersiz Akışı
            RabbitMQ-->>+Order.API: StockNotReservedEvent'i tüket
            Order.API->>Order.API: Sipariş durumunu 'Başarısız' (Failed) olarak güncelle
            deactivate Order.API
        end

    end

```

---

## Olaylar, Mesajlar ve Tüketiciler (Events, Messages & Consumers)

Bu projede iletişim, **Olaylar (Events)** üzerinden sağlanır. Bir olay, geçmişte olmuş ve değiştirilemez bir durumu ifade eder (örn. "Sipariş Oluşturuldu"). Bu olayları dinleyen ve belirli iş mantıklarını tetikleyen yapılara **Tüketici (Consumer)** denir.

### Paylaşımlı Varlıklar (`Shared` Projesi)

-   **Konum:** `Shared/`
-   Tüm olay ve mesaj tanımları burada yer alır.
    -   `Events/`: `OrderCreatedEvent`, `PaymentCompletedEvent` gibi tüm olayların sınıf tanımları.
    -   `Messages/`: Servisler arası doğrudan komut göndermek için kullanılabilecek mesaj tanımları (örn: `OrderItemMessage`).

### Olaylar ve Akışları

| Olay (Event) | Yayınlayan Servis (Publisher) | Tüketen Servis (Consumer) | Açıklama |
| :--- | :--- | :--- | :--- |
| `OrderCreatedEvent` | **Order.API** | **Stock.API** | Yeni bir siparişin oluşturulduğunu ve stok işlemlerinin başlaması gerektiğini bildirir. |
| `StockReservedEvent` | **Stock.API** | **Payment.API** | Sipariş için stokların başarıyla ayrıldığını ve ödeme sürecinin başlayabileceğini bildirir. |
| `StockNotReservedEvent` | **Stock.API** | **Order.API** | Yetersiz stok nedeniyle siparişin devam edemeyeceğini bildirir. Sipariş durumu 'Başarısız' olarak güncellenir. |
| `PaymentCompletedEvent`| **Payment.API** | **Order.API** | Ödemenin başarıyla tamamlandığını bildirir. Sipariş durumu 'Tamamlandı' olarak güncellenir. |
| `PaymentFailedEvent` | **Payment.API** | **Order.API** | Ödemenin başarısız olduğunu bildirir. Sipariş durumu 'Başarısız' olarak güncellenir. |

### Tüketiciler (Consumers)

Her servisin `Consumers/` klasöründe, belirli olayları dinleyen sınıflar bulunur.

-   **Order.API/Consumers/**
    -   `PaymentCompletedEventConsumer.cs`: Ödeme başarılı olayını dinler.
    -   `PaymentFailedEventConsumer.cs`: Ödeme başarısız olayını dinler.
    -   `StockNotReservedConsumer.cs`: Stok yok olayını dinler.

-   **Stock.API/Consumers/**
    -   `OrderCreatedEventConsumer.cs`: Sipariş oluşturuldu olayını dinler ve stok işlemlerini başlatır.

-   **Payment.API/Consumers/**
    -   `StockReservedEventConsumer.cs`: Stok ayrıldı olayını dinler ve ödeme işlemlerini başlatır.
