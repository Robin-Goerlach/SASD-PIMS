# Ready-to-Code Changeset

This repository overlay is intended to be applied before the first product-source commit.

## Adds

- `.gitattributes`;
- implementation baseline;
- version-controlled Lastenheft/Pflichtenheft Markdown sources and their embedded graphics;
- architecture v1.0 master, diagrams and registers;
- detailed development roadmap;
- database specification for P1/0.1;
- UI/UX specification for P1/0.1;
- test/acceptance specification;
- vertical-slice contract;
- initial commit plan;
- documentation index.

## Updates

- root README:
  - status becomes Ready-to-Code/pre-implementation;
  - Windows 11 x64 is the supported baseline;
  - licence correctly states Apache-2.0;
  - broken `LICENSE-DECISION.md` reference is removed;
  - documentation baseline links are added;
- architecture README points to the actual baseline;
- compact roadmap links to detailed planning;
- initial issue backlog records the completed P0 documentation block and evidence-based publish comparison;
- changelog records the baseline.

## Deliberately not added

- application source code;
- fake implementation screenshots;
- CI workflow before a solution exists;
- installer/update framework;
- new third-party packages;
- generated PDF/DOCX copies of source documents;
- complete future 1.0 physical database schema.

## Suggested commit

```text
chore(repo): establish ready-to-code implementation baseline
```

Suggested body:

```text
Version the approved requirements and architecture sources, add the
vertical-slice database/UI/test specifications, define repository
line-ending policy, and align README licence/platform statements.

Prepare milestone 0.0.1-internal without adding product code or
premature runtime dependencies.
```
