# File Header Convention

Every file you create or modify must include a standardized header comment.

---

## C# Files

Place after using statements, before namespace:

```csharp
// ============================================================================
// <FileName>.cs
// Purpose: <Specific one-sentence description>
// Dependencies: <Key injected services, comma-separated, or "None">
// ============================================================================
```

---

## Razor Views

Place after @page/@model directives:

```razor
@* ============================================================================
   <FileName>.cshtml
   Purpose: <What this view renders>
   ============================================================================ *@
```

---

## Rules

- Be specific: "Generates PDF invoices with bilingual EN/FR headers using QuestPDF" not "Handles invoices"
- Keep Purpose under 20 words
- Always include this header when creating new files
- Update the header if you significantly change a file's purpose

---

## Examples

| Bad | Good |
|-----|------|
| "Service for emails" | "Sends transactional emails for permit approvals, invoice delivery, and password resets via MailKit" |
| "Invoice page" | "Displays invoice details with payment history, download PDF link, and outstanding balance" |
| "Helper class" | "Calculates overtime hours based on subdivision-specific rules and Canadian statutory holidays" |
