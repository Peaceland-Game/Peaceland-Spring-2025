using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peaceland.Notebook
{
    public enum NotebookHarnessSeverity
    {
        Error = 0,
        Warning = 1,
        Info = 2,
    }

    [Serializable]
    public sealed class NotebookHarnessIssue
    {
        public NotebookHarnessSeverity severity;
        public string code;
        public string entryId;
        public string message;

        public NotebookHarnessIssue(NotebookHarnessSeverity severity, string code, string message, string entryId = null)
        {
            this.severity = severity;
            this.code = code;
            this.message = message;
            this.entryId = entryId;
        }
    }

    public sealed class NotebookHarnessReport
    {
        public readonly List<NotebookHarnessIssue> Issues = new List<NotebookHarnessIssue>();
        public string generatedUtc;
        public int databaseEntryCount;
        public int catalogSpecCount;
        public int wiredCollectCount;

        public bool HasErrors => Issues.Any(issue => issue.severity == NotebookHarnessSeverity.Error);
        public bool HasWarnings => Issues.Any(issue => issue.severity == NotebookHarnessSeverity.Warning);
        public bool Passed => !HasErrors;

        public int ErrorCount => Issues.Count(issue => issue.severity == NotebookHarnessSeverity.Error);
        public int WarningCount => Issues.Count(issue => issue.severity == NotebookHarnessSeverity.Warning);

        public void Add(NotebookHarnessSeverity severity, string code, string message, string entryId = null)
        {
            Issues.Add(new NotebookHarnessIssue(severity, code, message, entryId));
        }

        public string ToMarkdown()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Notebook Harness Report");
            builder.AppendLine();
            builder.AppendLine("- Generated (UTC): `" + (generatedUtc ?? "unknown") + "`");
            builder.AppendLine("- Database entries: " + databaseEntryCount);
            builder.AppendLine("- Catalog specs: " + catalogSpecCount);
            builder.AppendLine("- Wired collect triggers: " + wiredCollectCount);
            builder.AppendLine("- Result: **" + (Passed ? "PASS" : "FAIL") + "** (" + ErrorCount + " errors, " + WarningCount + " warnings)");
            builder.AppendLine();

            AppendSection(builder, NotebookHarnessSeverity.Error);
            AppendSection(builder, NotebookHarnessSeverity.Warning);
            AppendSection(builder, NotebookHarnessSeverity.Info);

            if (Issues.Count == 0)
            {
                builder.AppendLine("No issues found.");
            }

            return builder.ToString();
        }

        private void AppendSection(StringBuilder builder, NotebookHarnessSeverity severity)
        {
            List<NotebookHarnessIssue> filtered = Issues.Where(issue => issue.severity == severity).ToList();
            if (filtered.Count == 0)
            {
                return;
            }

            builder.AppendLine("## " + severity);
            builder.AppendLine();
            for (int i = 0; i < filtered.Count; i++)
            {
                NotebookHarnessIssue issue = filtered[i];
                string idPart = string.IsNullOrWhiteSpace(issue.entryId) ? string.Empty : " `" + issue.entryId + "` — ";
                builder.AppendLine("- **" + issue.code + "**: " + idPart + issue.message);
            }

            builder.AppendLine();
        }
    }
}
