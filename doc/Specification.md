# **ExamEngine — .NET MAUI Greenfield Technical Specification**

*Last updated: 2025-09-02*

**Goal:** Deliver a brand‑new, offline‑first exam preparation application built **exclusively on .NET MAUI** for iOS and Android, with a modern, testable architecture and a consistent, minimal UX optimized for practice and full exam simulation.  
---

## **1\) Product Overview**

**Vision**: A lean, white‑labelable exam prep engine that can power multiple certification apps (CBAP, PMP, Azure, etc.) from the same codebase through configuration and content packs.

**Core Experience**

* **Dashboard**: progress tiles, topic mastery, readiness score, streaks.

* **Practice**: Adaptive learning, Quick practice, Full exam simulation.

* **Question Player**: Exhibits, flag/review, timer, explanations.

* **Review & Insights**: attempt results, topic breakdown, heatmaps.

* **Settings & Legal**: theme, font scale, data export/reset, privacy/terms/support.

**Primary Platforms**: iOS 15+, Android 8.0+

**Target Runtime**: .NET 8 (MAUI single‑project)

---

## **2\) Scope & Goals**

* Offline‑first: all core flows function without network after initial content install.

* Content‑driven: exams, topics, and questions loaded from signed **content packs**.

* Accessibility: WCAG 2.1 AA color contrast, 44pt targets, Dynamic Type to 200%.

* Performance: \<3s cold start; \<500ms question display; \<100ms DB queries typical.

* Privacy: no PII by default; all learning data stored locally unless exported by user.

---

## **3\) Architecture (MAUI‑native)**

* **Pattern**: MVVM using CommunityToolkit.Mvvm (source‑generated INotifyPropertyChanged, \[RelayCommand\]).

* **Navigation**: Shell routes with deep links and modal flows where needed.

* **Dependency Injection**: Microsoft.Extensions.DependencyInjection configured in MauiProgram.

* **Persistence**: SQLite via sqlite-net-pcl; versioned SQL migrations; WAL mode.

* **Serialization**: System.Text.Json (AOT‑friendly; Utf8JsonReader for streaming).

* **Theming**: XAML ResourceDictionary with light/dark palettes and tokenized spacing/radii.

* **Images/Exhibits**: App‑data storage; FileImageSource with caching.

* **Logging**: Microsoft.Extensions.Logging with platform sinks (Debug/Console/AppCenter optional).

### **Project Structure**

/src  
  /App            \# App.xaml, AppShell, DI composition root  
  /DesignSystem   \# Tokens, styles, controls (Button, Card, ProgressBar)  
  /Domain         \# Entities, value objects, algorithms (scoring, EWMA)  
  /Data           \# SQLite, migrations, repositories  
  /Features  
    /Dashboard  
    /Practice  
    /Exam  
    /Review  
    /Tips  
    /Settings  
  /Common         \# Converters, behaviors, validators, timers  
/tests  
  /Unit           \# xUnit \+ FluentAssertions  
  /Database       \# SQLite migrations & repo tests  
  /E2E            \# Appium smoke tests (iOS/Android)  
---

## **4\) Design System (Tokens → XAML)**

Minimal, high‑contrast palette with accessible defaults.

\<\!-- Resources/Theme.xaml \--\>  
\<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"\>  
  \<\!-- Colors \--\>  
  \<Color x:Key="Color.Primary"\>\#1976D2\</Color\>  
  \<Color x:Key="Color.Accent"\>\#F57C00\</Color\>  
  \<Color x:Key="Color.Success"\>\#43A047\</Color\>  
  \<Color x:Key="Color.Text"\>\#111111\</Color\>  
  \<Color x:Key="Color.Surface"\>\#FFFFFF\</Color\>  
  \<Color x:Key="Color.SurfaceDark"\>\#121212\</Color\>

  \<\!-- Spacing & Radii \--\>  
  \<x:Double x:Key="Space.XS"\>4\</x:Double\>  
  \<x:Double x:Key="Space.S"\>8\</x:Double\>  
  \<x:Double x:Key="Space.M"\>12\</x:Double\>  
  \<x:Double x:Key="Radius.M"\>12\</x:Double\>

  \<\!-- Typography base style \--\>  
  \<Style x:Key="Text.Body" TargetType="Label"\>  
    \<Setter Property="FontSize" Value="14" /\>  
    \<Setter Property="TextColor" Value="{StaticResource Color.Text}" /\>  
  \</Style\>  
\</ResourceDictionary\>

**A11y notes**: 44×44dp min touch target, SemanticProperties.Description, AutomationProperties.Name, color contrast ≥ 4.5:1, AppThemeBinding parity.

---

## **5\) Data Layer & Schema**

**Library**: sqlite-net-pcl

**Path**: FileSystem.AppDataDirectory/examengine.db3

**Migrations**: embedded SQL files migrations/00X\_\*.sql; record applied scripts in schema\_migrations.

**Tables (initial)**

* exams — exam metadata (id, name, version, blueprint, timing).

* topics — taxonomy with weights and ordering.

* questions — flexible question content (type, body, explanation, difficulty, tags).

* exhibits — media assets (uri, alt text, hash).

* question\_exhibits — question↔exhibit linking with order.

* attempts — practice/exam sessions.

* attempt\_items — per‑question responses, correctness, timing, flags.

* progress — per‑topic aggregates (attempted, correct, EWMA proficiency, streaks).

* user\_preferences — key/value for app settings.

* content\_packs — installed pack versions and checksums.

**Database bootstrap (sketch)**

public sealed class DatabaseManager : IDatabaseManager  
{  
    readonly string \_dbPath \= Path.Combine(FileSystem.AppDataDirectory, "examengine.db3");  
    public Task InitializeAsync()  
    {  
        using var conn \= new SQLiteConnection(\_dbPath);  
        conn.Execute("PRAGMA foreign\_keys \= ON; PRAGMA journal\_mode \= WAL;");  
        ApplyMigrations(conn, GetEmbeddedScripts());  
        return Task.CompletedTask;  
    }  
}  
---

## **6\) Domain Model (examples)**

public enum QuestionType { SingleChoice, MultipleChoice, TrueFalse }

public sealed class Question  
{  
    public required string Id { get; init; }  
    public required string ExamId { get; init; }  
    public required string TopicId { get; init; }  
    public required QuestionType Type { get; init; }  
    public required string Body { get; init; }   // Markdown supported  
    public string? Explanation { get; init; }  
    public string\[\] Choices { get; init; } \= Array.Empty\<string\>();  
    public int\[\] CorrectIndexes { get; init; } \= Array.Empty\<int\>();  
}

Repositories expose async methods to query exams, questions, attempts, progress, and to sample question sets by topic/difficulty/type with exclusions.

---

## **7\) Content Packs**

* **Format**: .zip with manifest.json (exam id, version, checksum, counts), questions.jsonl, and exhibits/.

* **Install**: user selects file → verify checksum → unzip → import to DB idempotently → record in content\_packs.

* **Fixture**: include a tiny pack (cbap: 1 topic, 1 single‑choice question) for offline smoke tests.

---

## **8\) Features & UX**

**Navigation**: AppShell routes (//home, practice/:examId, exam/:attemptId, review/:attemptId).

**Home**: quick actions, readiness, streaks, topic tile list.

**Practice**:

* **Adaptive Learning** (EWMA proficiency \+ blueprint weights; exclude items seen in last 24h).

* **Quick Practice** (topics, count, difficulty, timer optional).

* **Full Exam** (exact count & timing; flag/review; pre‑submit checklist).

**Question Player**: header (Q counter, flag, timer), content (stem, exhibits), footer (prev/next/submit, progress bar). Markdown rendered via Markdig → FormattedString or guarded WebView.

**Review & Insights**: score, pass/fail, topic breakdown, time per question, explanations.

**Settings**: theme, font scale, data import/export, reset, analytics toggle.

---

## **9\) State, Performance, Telemetry**

* **State**: MVVM ObservableObject, \[ObservableProperty\], \[RelayCommand\]; WeakReferenceMessenger for cross‑feature events.

* **Performance**: AOT+LLVM (iOS), R8/D8 (Android), image pre‑sizing, prepared statements; avoid large allocations (stream JSON).

* **Telemetry (optional)**: Adobe Analytics bridge; toggle in Settings; no PII.

---

## **10\) Security, Privacy, and Store Compliance**

* **Privacy by default**: no accounts required; local‑only learning data; export/reset in Settings.

* **Content integrity**: Ed25519 signature \+ SHA‑256 checksum for packs.

* **Compliance screens** (reachable from Settings/About):

  * **Privacy Policy**, **Terms of Service**, **Support/Contact** links.

  * Purchases/subscriptions screen (if IAP added later) with platform manage links.

  * Visible exam disclaimer: “Not affiliated with the certification owner; trademarks belong to their owners.”

---

## **11\) Testing Strategy**

* **Unit**: xUnit \+ FluentAssertions for ViewModels, scoring, parsers.

* **Database**: run migrations on temp DB; verify tables/indexes; CRUD & sampling behavior.

* **E2E**: Appium smoke — install pack → start Quick (5Q) → submit → see results → dashboard updates.

* **Accessibility**: automated token contrast checks; manual VoiceOver/TalkBack runs; Dynamic Type to 200%.

---

## **12\) Build & CI**

* **Pipeline**: GitHub Actions / Azure DevOps — restore → build → unit → DB → package → E2E smoke.

* **Artifacts**: .ipa (TestFlight) and .aab (Play Console). Store secrets secured.

---

## **13\) Implementation Plan (6–8 weeks)**

**Week 1** — Bootstrap MAUI app, DI, Shell, tokens/styles, baseline pages.

**Week 2** — SQLite layer, migration runner, 001\_initial.sql, fixture seeding.

**Week 3** — Practice flows (Adaptive/Quick), question renderer (single‑choice \+ explanation).

**Week 4** — Full Exam mode (timer, flag/review, autosave), scoring, Review screen.

**Week 5** — Progress service (EWMA), Dashboard tiles, Settings (theme, scale, data mgmt).

**Week 6** — Unit/DB tests complete; Appium smoke; perf pass; a11y audit.

**Weeks 7–8** — Beta hardening, telemetry toggle, polish, store readiness.

---

## **14\) Acceptance Criteria**

* App launches offline; fixture content pack installs successfully.

* Start Adaptive/Quick/Full Exam; answer; submit; view explanations; review analytics.

* Progress updates (per‑topic accuracy, proficiency, streaks) visible on Dashboard.

* A11y: labels present; dynamic text works; contrast AA.

* Perf: \<3s cold start (mid‑range devices); \<500ms question render; DB queries \<100ms typical.

* Tests: \>70% VM/domain coverage; DB migration tests pass; E2E smoke green.

---

## **15\) Appendix**

### **Example SQL (001\_initial.sql — excerpt)**

CREATE TABLE IF NOT EXISTS schema\_migrations (id INTEGER PRIMARY KEY, script TEXT NOT NULL UNIQUE, applied\_at TEXT NOT NULL);  
CREATE TABLE IF NOT EXISTS exams (  
  id TEXT PRIMARY KEY,  
  name TEXT NOT NULL,  
  version TEXT,  
  blueprint TEXT,  
  time\_limit\_minutes INTEGER,  
  passing\_score REAL  
);  
CREATE TABLE IF NOT EXISTS topics (  
  id TEXT PRIMARY KEY,  
  exam\_id TEXT NOT NULL,  
  name TEXT NOT NULL,  
  weight REAL,  
  "order" INTEGER,  
  FOREIGN KEY (exam\_id) REFERENCES exams(id) ON DELETE CASCADE  
);  
CREATE TABLE IF NOT EXISTS questions (  
  id TEXT PRIMARY KEY,  
  exam\_id TEXT NOT NULL,  
  topic\_id TEXT NOT NULL,  
  type INTEGER NOT NULL,  
  body TEXT NOT NULL,  
  explanation TEXT,  
  difficulty INTEGER,  
  tags TEXT,  
  FOREIGN KEY (exam\_id) REFERENCES exams(id) ON DELETE CASCADE,  
  FOREIGN KEY (topic\_id) REFERENCES topics(id) ON DELETE CASCADE  
);  
CREATE TABLE IF NOT EXISTS exhibits (  
  id TEXT PRIMARY KEY,  
  uri TEXT NOT NULL,  
  alt\_text TEXT,  
  hash TEXT  
);  
CREATE TABLE IF NOT EXISTS question\_exhibits (  
  question\_id TEXT NOT NULL,  
  exhibit\_id TEXT NOT NULL,  
  "order" INTEGER NOT NULL DEFAULT 0,  
  PRIMARY KEY (question\_id, exhibit\_id),  
  FOREIGN KEY (question\_id) REFERENCES questions(id) ON DELETE CASCADE,  
  FOREIGN KEY (exhibit\_id) REFERENCES exhibits(id) ON DELETE CASCADE  
);  
CREATE TABLE IF NOT EXISTS attempts (  
  id TEXT PRIMARY KEY,  
  exam\_id TEXT NOT NULL,  
  started\_at TEXT NOT NULL,  
  finished\_at TEXT,  
  mode TEXT NOT NULL,  
  duration\_sec INTEGER NOT NULL DEFAULT 0,  
  FOREIGN KEY (exam\_id) REFERENCES exams(id) ON DELETE CASCADE  
);  
CREATE TABLE IF NOT EXISTS attempt\_items (  
  attempt\_id TEXT NOT NULL,  
  question\_id TEXT NOT NULL,  
  user\_answer TEXT,  
  is\_correct INTEGER NOT NULL DEFAULT 0,  
  time\_sec INTEGER NOT NULL DEFAULT 0,  
  PRIMARY KEY (attempt\_id, question\_id),  
  FOREIGN KEY (attempt\_id) REFERENCES attempts(id) ON DELETE CASCADE,  
  FOREIGN KEY (question\_id) REFERENCES questions(id) ON DELETE CASCADE  
);  
CREATE TABLE IF NOT EXISTS progress (  
  exam\_id TEXT NOT NULL,  
  topic\_id TEXT,  
  correct\_count INTEGER NOT NULL DEFAULT 0,  
  total\_count INTEGER NOT NULL DEFAULT 0,  
  last\_practiced\_at TEXT,  
  PRIMARY KEY (exam\_id, topic\_id)  
);  
CREATE TABLE IF NOT EXISTS user\_preferences (  
  key TEXT PRIMARY KEY,  
  value TEXT NOT NULL  
);  
CREATE TABLE IF NOT EXISTS content\_packs (  
  id TEXT PRIMARY KEY,  
  name TEXT NOT NULL,  
  version TEXT,  
  manifest\_json TEXT NOT NULL,  
  installed\_at TEXT NOT NULL  
);

### **AppShell routes (illustrative)**

\<Shell ...\>  
  \<TabBar\>  
    \<ShellContent Title="Home" Route="home" ContentTemplate="{DataTemplate views:HomePage}" /\>  
    \<ShellContent Title="Practice" Route="practice" ContentTemplate="{DataTemplate views:PracticePage}" /\>  
  \</TabBar\>  
  \<ShellContent Route="exam" ContentTemplate="{DataTemplate views:ExamPage}" /\>  
  \<ShellContent Route="review" ContentTemplate="{DataTemplate views:ReviewPage}" /\>  
\</Shell\>

### **MauiProgram (composition root — excerpt)**

var builder \= MauiApp.CreateBuilder();  
builder  
  .UseMauiApp\<App\>()  
  .ConfigureFonts(fonts \=\> fonts.AddFont("Inter-Regular.ttf", "Inter"));

builder.Services  
  .AddSingleton\<IDatabaseManager, DatabaseManager\>()  
  .AddSingleton\<IQuestionRepository, QuestionRepository\>()  
  .AddSingleton\<IAttemptRepository, AttemptRepository\>()  
  .AddSingleton\<IProgressService, ProgressService\>()  
  .AddSingleton\<ContentPackService\>();

return builder.Build();  
