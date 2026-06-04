# NurFlac Telegram Bot - Architectural Specification & Refactoring Guide

## 1. System Overview
**NurFlac** is a C#-based Telegram bot designed to run on a Linux environment. Its primary function is to accept, validate, and track lossless audio file uploads while enforcing user access controls (timeouts, bans, admin privileges) and managing complex upload states (e.g., batch album uploads). 

## 2. Technical Stack
* **Language:** C# (.NET 9.0)
* **OS Target:** Linux (Dockerized or systemd service)
* **Database:** SQLite (via Entity Framework Core 9 or Dapper)
* **Libraries:** `Telegram.Bot`, `FFMpegCore` (cross-platform audio analysis), `TagLib#` (for MIME/metadata extraction).

## 3. Core Features & Business Logic
1. **Access Control:**
   * Global checks on all interactions: Validate if a user is timed out or banned.
   * Admins are designated exclusively via Environment Variables. Admins bypass standard timeouts and can issue `/ban`, `/unban`, `/timeout` commands.
2. **Audio Validation Pipeline:**
   * Step 1: File Extension Check (e.g., `.flac`, `.wav`, `.alac`).
   * Step 2: MIME/Metadata Check (Must reflect a mathematically lossless format).
   * Step 3: Spectral Analysis (Verifies the audio reaches the 20kHz frequency threshold to prevent upsampled lossy files using an FFmpeg interface).
3. **Command Execution:**
   * **Normal Commands:** Pre-checked for timeouts/bans.
   * **Admin Commands:** Pre-checked for Admin authority.
4. **Album Upload Mode:**
   * Initiated by `/album-upload`. The bot enters a waiting state for the user.
   * Accumulates files until `/album-done` is received.
   * Processes all accumulated files through the validation pipeline.
   * Generates a final upload report detailing success/failures per file.
5. **Database Tracking:**
   * SQLite persistence for tracking successful uploads.
   * Schema requires tracking: `FileName`, `FileHash` (SHA-256), and `UploadDate`.

## 4. Design Pattern Implementation Plan
The architecture mandates exactly 12 design patterns. Below is the mapping of each pattern to its specific use case in the bot.

### Creational Patterns (4)
1. **Singleton:** `ConfigManager` - Ensures a single, globally accessible instance for reading the configuration file and Environment Variables (for Admin settings). Uses modern .NET 9 thread-safe lazy initialization.
2. **Factory Method:** `BotCommandFactory` - Creates specific command objects (`BanCommand`, `AlbumUploadCommand`) based on the incoming Telegram message text.
3. **Builder:** `AlbumReportBuilder` - Constructs the complex multi-file upload report step-by-step as files pass or fail validation.
4. **Abstract Factory:** `AudioValidatorFactory` - Creates families of related validation objects depending on the exact audio codec detected.

### Structural Patterns (4)
1. **Decorator:** `CommandAuthorizationDecorator` - Wraps base commands to add behavior (e.g., checking if the user is an admin or timed out) before executing the underlying command logic.
2. **Facade:** `AudioProcessingFacade` - Provides a simplified, high-level interface to the complex validation pipeline (Extension -> Metadata -> Frequency analysis).
3. **Adapter:** `FFmpegAdapter` - Wraps the third-party audio analysis tool/library (used for the 20kHz check) to match the bot's internal `IAudioAnalyzer` interface.
4. **Proxy:** `DatabaseAccessProxy` - Controls access to the SQLite repository, potentially adding caching (utilizing .NET 9's HybridCache if applicable) for frequently accessed data like user ban status to reduce DB hits.

### Behavioral Patterns (4)
1. **Chain of Responsibility:** `ValidationPipeline` - Passes an uploaded file through a chain of handlers: Extension Handler -> Mime Handler -> Frequency Handler. If one fails, the chain breaks and returns an error.
2. **State:** `UserSessionState` - Manages user context. A user transitions from `IdleState` to `AlbumUploadState` upon `/album-upload`, altering how the bot interprets incoming files/messages.
3. **Command:** `IBotCommand` - Encapsulates requests as objects (e.g., `TimeoutUserCommand`, `ProcessFileCommand`), allowing for queuing, logging, and decoupling the invoker from the receiver.
4. **Strategy:** `IHashStrategy` - Encapsulates the hashing algorithm used for the database file tracking, allowing the system to easily swap from MD5 to SHA256 without altering the calling code.

## 5. UML Documentation Requirements
* A folder named `/UML_Diagrams` must be created at the root of the repository.
* Every design pattern implemented must have an accompanying UML 2.5 compliant Class Diagram.
* Diagrams should be created using standard text-to-UML tools (e.g., PlantUML).
* Diagrams must include descriptive comments detailing the pattern's role.
* CI/CD or PR guidelines must state that any modification to pattern classes requires an update to the respective diagram.

## 6. Database Schema (SQLite)
**Table: UploadedFiles**
* `Id` (INTEGER PRIMARY KEY)
* `FileName` (TEXT)
* `FileHash` (TEXT UNIQUE)
* `UploadDate` (DATETIME)
