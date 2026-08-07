# Screenshot and Demo Concept

This directory distinguishes strictly between **design concepts** and **verified screenshots of implemented functionality**.

`00-ui-concept-project-dashboard.png` is a generated design concept. It may be displayed in the repository README only when it is clearly labelled as a concept and not as proof of implementation.

Real product screenshots must not be added before the corresponding function actually works.

## Principles

- use only synthetic/demo project data;
- never show personal documents, chats, tokens, filesystem paths or real customer/project information;
- never present a design mock-up as implemented functionality;
- concept images must use a `00-ui-concept-*` filename and an explicit concept caption;
- verified screenshots must capture the actual application;
- keep a consistent window size and Windows scaling;
- prefer PNG for static UI screenshots;
- crop deliberately but retain enough application chrome to prove context;
- provide meaningful alt text in README/documentation;
- update or remove stale screenshots when the UI changes materially.

## Current concept image

| File | Status | Purpose |
| --- | --- | --- |
| `00-ui-concept-project-dashboard.png` | Design concept | Visual direction only; includes capabilities from later roadmap stages |

The concept is **non-binding**. In particular, its ribbon-like command area does not approve a Ribbon dependency or an external UI framework.

## Planned verified screenshot set

| File | Earliest version | Content |
| --- | --- | --- |
| `01-project-catalog.png` | 0.1.0 | Main project list with synthetic projects |
| `02-project-editor.png` | 0.1.0 | Create/edit project |
| `03-status-review.png` | 0.2.0 | Status/review view |
| `04-requirements.png` | 0.3.0 | Requirement list/detail |
| `05-references.png` | 0.3.0 | Typed repository/chat/document references |
| `06-search.png` | 0.4.0 | Search/filter result |
| `07-import-preview.png` | 0.4.0 | Safe import preview |
| `08-backup-restore.png` | 0.5.0 | Verified backup/restore workflow |

These names are reserved placeholders. The files themselves should not exist until screenshots can truthfully be captured.

## README hero screenshot

Add one hero screenshot only when `0.1.0` is stable enough that the main window represents the intended product.

Recommended:

- 1400–1800 px wide;
- 16:9 or similar landscape ratio;
- 100% or 125% DPI;
- one realistic but synthetic data set;
- no annotations baked into the screenshot unless the documentation needs them.

## Demo

After `0.1.0`, consider a short 15–30 second demo showing:

1. open project catalog;
2. create a synthetic project;
3. save it;
4. close/reopen or return to list;
5. show persisted result.

Prefer a small GIF only if file size and text readability remain acceptable. Otherwise host a short MP4 in a GitHub Release and keep the README to static screenshots.

Later demos should be feature-specific instead of one long marketing video.

## Accessibility

Every embedded screenshot in Markdown should have useful alt text. Important product information must never be available only through the screenshot.
