# SASD PIMS Architecture

The approved architecture baseline is versioned in this repository under:

```text
docs/architecture/baseline-v1.0/
├── documents/
├── diagrams/
└── register/
```

Primary entry point:

[`baseline-v1.0/documents/SASD-PIMS-Software-Architecture-Document-v1.0.md`](baseline-v1.0/documents/SASD-PIMS-Software-Architecture-Document-v1.0.md)

The baseline contains:

- the architecture master document;
- Mermaid diagram sources;
- rendered SVG/PNG diagrams;
- ADR, quality-gate, traceability, open-decision and technical-debt registers.

The eight historical detail volumes remain part of the architecture source package outside this lean repository snapshot. The master document is the repository's implementation entry point; when a detail-volume rule becomes relevant to code and is not represented adequately in the master/registers, import the corresponding editable source rather than relying on a PDF-only copy.

Architecture status and implementation status are separate. A decision may be accepted architecturally while still lacking implementation evidence.

New cross-cutting decisions should be recorded through ADR/update of the relevant register rather than by silently changing code.
