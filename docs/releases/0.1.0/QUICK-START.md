# SASD PIMS 0.1.0 — Quick Start

1. Extract `SASD-PIMS-0.1.0-win-x64.zip` to a writable local directory.
2. Start `Sasd.Pims.WinForms.exe`. No separate .NET runtime or database server is required.
3. Use **Neu** to create a project. The project key is stable after creation.
4. Select a project to view it; use **Bearbeiten** to change master data.
5. Use **Archivieren** to remove a project from the active list without deleting it. Enable
   **Archivierte anzeigen** and use **Reaktivieren** to return it.
6. Use **Sicherung** regularly and store the ZIP separately. **Wiederherstellen** validates a
   selected backup and creates a rollback backup before replacing active data.

Application data is stored in `%LOCALAPPDATA%\SASD\PIMS`. Project texts may contain sensitive
business information; choose export and backup locations accordingly.
