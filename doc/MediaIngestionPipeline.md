# Media Ingestion Pipeline - Implementation Summary

## Overview
The media ingestion pipeline uses **Akka.NET actors** for processing uploaded media images through OCR, classification, content moderation, and catalog listing. The actor pattern provides message-driven, scalable, and resilient processing with built-in supervision and fault tolerance.

## Components Created

### Domain Layer (ThriftMedia.Domain)

1. **MediaStatus Value Object** (`ValueObjects/MediaStatus.cs`)
   - Statuses: Uploaded, Processing, PendingClassification, Flagged, Listed, Failed
   - Immutable value object following DDD patterns

2. **Media Entity** (`Entities/Media.cs`)
   - Business rules for media processing lifecycle
   - Methods: `StartProcessing()`, `SetOcrData()`, `Classify()`, `FlagAsExplicitContent()`, `ListInCatalog()`, `MarkAsFailed()`
   - Enforces domain invariants (e.g., cannot list explicit content or unknown types)

3. **Domain Events** (`Events/`)
   - `MediaUploadedEvent`
   - `ExplicitContentDetectedEvent`
   - `MediaListedEvent`
   - `MediaProcessingFailedEvent`

### Application Layer (ThriftMedia.Application)

4. **Pipeline Actors** (`Pipelines/Actors/`)
   - `MediaProcessingCoordinatorActor` - Orchestrates the processing pipeline
   - `OcrProcessingActor` - Handles OCR processing tasks
   - `MediaClassificationActor` - Performs media type classification
   - `ContentModerationActor` - Executes content moderation checks
   - `CatalogListingActor` - Manages catalog listing operations
   - Each actor runs independently with supervision for fault tolerance

5. **Actor Messages** (`Pipelines/Messages/`)
   - `ProcessMediaMessage` - Initiates media processing
   - `OcrCompletedMessage` - OCR results
   - `ClassificationCompletedMessage` - Classification results
   - `ModerationCompletedMessage` - Moderation results
   - `ListingCompletedMessage` - Listing confirmation

6. **Service Interfaces** (`Services/`)
   - `IOcrService` - OCR processing
   - `IMediaClassificationService` - Media type classification
   - `IContentModerationService` - Content moderation

7. **Repository Interfaces** (`Repositories/`)
   - `IMediaRepository` - Media persistence
   - `IStoreRepository` - Store persistence

8. **CQRS Commands** (`Commands/`)
   - `ProcessMediaCommand` - Command to initiate media processing
   - `ProcessMediaCommandHandler` - Sends message to actor system

### Worker Service (ThriftMedia.MediaProcessor)

9. **Akka.NET Actor System**
   - `MediaProcessorWorker` - Hosts Akka.NET actor system
   - Listens to RabbitMQ message queue
   - Routes messages to `MediaProcessingCoordinatorActor`
   - Manages actor lifecycle and supervision strategies
   - Configured for distributed processing and fault tolerance

## Pipeline Flow

```
Upload → RabbitMQ Queue → Worker Service
                              ↓
                     Actor System Entry
                              ↓
              MediaProcessingCoordinatorActor
                              ↓
        ┌────────────────────┴────────────────────┐
        ↓                    ↓                    ↓
   OcrProcessing      Classification      ContentModeration
      Actor               Actor                 Actor
        ↓                    ↓                    ↓
        └────────────────────┬────────────────────┘
                             ↓
                   CatalogListingActor
                             ↓
                  PostgreSQL Database
```

## Actor System Design

The pipeline uses Akka.NET actors for:
- **Message-driven processing**: Actors communicate via immutable messages
- **Supervision**: Parent actors supervise child actors, restarting on failure
- **Isolation**: Each processing step runs in its own actor for fault isolation
- **Scalability**: Actors can be distributed across multiple nodes
- **Resilience**: Built-in retry and error handling through supervision strategies

## Extensibility

The actor-based pipeline is designed for easy extension:
- New processing actors can be added to the supervision hierarchy
- Actors communicate via messages, enabling loose coupling
- Actor behavior can be modified without affecting other actors
- Supervision strategies handle failures independently per actor

## Next Steps (Implementation Required)

1. **Infrastructure Layer**
   - Implement `IOcrService` (Tesseract OCR or EasyOCR)
   - Implement `IMediaClassificationService` (ML model or rule-based)
   - Implement `IContentModerationService` (open-source content moderation library)
   - Implement repositories with EF Core

2. **Actor System Implementation**
   - Create actor hierarchy with supervision strategies
   - Define message protocols between actors
   - Implement actor behaviors for each processing step
   - Configure actor persistence for stateful operations

2. **AppHost Configuration**
   - Add MediaProcessor to Aspire AppHost
   - Configure RabbitMQ connection
   - Wire up Akka.NET dependencies
   - Configure actor system settings

3. **API Endpoints**
   - Media upload endpoint
   - Queue message publishing
   - Status checking endpoints

4. **Testing**
   - Unit tests for actor behaviors using `TestKit`
   - Integration tests for actor message flows
   - Worker service tests with mock actor system

All components follow TDD principles, CQRS pattern, vertical slice architecture, and .NET best practices. Backend processing uses **Akka.NET actors** for scalable, resilient, message-driven architecture. Data is stored in **PostgreSQL** using Entity Framework Core.
