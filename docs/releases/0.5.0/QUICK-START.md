# SASD PIMS 0.5.0 RC — Quick Start and operations

Verify the ZIP SHA-256, extract it to a writable local directory and start `Sasd.Pims.WinForms.exe`. No installer, cloud account, web service or separate .NET runtime is required. Data is stored locally in `%LOCALAPPDATA%\SASD\PIMS\data\pims.db`; logs are under `logs`, and migration/rollback backups under `backups`. Help → **Betriebsinformationen und Speicherorte** shows the effective paths read-only.

Create a Project, then use **Steuerung**, **Blocker**, **Anforderungen**, Ctrl+F search and **Projekt → Traceability/Änderungsverlauf**. External references are pointers only: PIMS does not copy their content. Store no credentials; unsafe schemes and recognizable secrets are rejected. Missing local targets are reported without deleting the reference.

Use **Sicherung** before risky work and keep the chosen ZIP outside the application directory. Restore validates manifest, SHA-256, schema and SQLite integrity in staging before replacing active data and retains rollback evidence. For disaster recovery, install/extract the same compatible PIMS version in an empty profile, start once, then restore the last verified backup. Never replace `pims.db` manually while PIMS runs.

JSON and Markdown export destinations are selected per operation. JSON exchange 1.0 is export-only; import is not available in this MVP. Local absolute reference targets remain machine-local.

For troubleshooting, copy the displayed Error ID and locate the corresponding JSONL log entry; logs intentionally omit full project content and secrets. If startup fails after damage, preserve the data directory, do not overwrite it, and restore only from a verified backup.

To remove the portable application, close PIMS and delete the extracted application directory. Local user data under `%LOCALAPPDATA%\SASD\PIMS` is deliberately separate; remove it only after a verified backup and only when permanent data deletion is intended.

Known limits: 200 visible search results without interactive paging; no import, provider API, synchronization, embedded files or multi-user mode. Physical DPI, offline, clean-system, endurance and usability acceptance remain listed in `MANUAL-ACCEPTANCE.md`.
