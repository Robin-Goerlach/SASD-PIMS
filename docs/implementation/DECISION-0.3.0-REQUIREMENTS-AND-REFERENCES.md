# SASD PIMS — Decision Note 0.3.0: Requirements and typed references

**Status:** Approved for implementation  
**Milestone:** `0.3.0 — Requirements and typed references`  
**Date:** 2026-08-20

## Purpose

This decision fixes the Requirement and ExternalReference semantics required for 0.3.0. PIMS records concise project context and provenance while external systems remain authoritative for tasks, documents, repositories and conversations.

## Requirement

A Requirement belongs to exactly one Project and has a stable GUID plus an immutable, automatically allocated project-local key `REQ-001`, `REQ-002`, and so on. The database unique constraint on `(ProjectId, Key)` is authoritative. Allocated numbers are never reused and there is no global Requirement sequence.

Controlled priorities are `Must`, `Should` and `Could`. `NotPlanned` is not a priority.

Controlled decision states are `Proposed`, `Approved`, `Deferred` and `Rejected`. New Requirements start as `Proposed`. `Deferred` and `Rejected` require a non-empty decision reason. Priority and decision state are independent. Requirements are not hard-deleted in 0.3.0.

## Acceptance criteria

An AcceptanceCriterion has `Id`, `RequirementId`, persistent `Sequence`, `Text` and optional `VerificationReferenceId`. Every `Must` Requirement has at least one criterion. Criteria do not carry completion or execution state and are not tasks or test cases.

## Source

Every Requirement has one controlled SourceType: `Internal`, `Stakeholder`, `Regulatory`, `Research`, `ExistingSystem`, `External` or `Other`. `SourceDate`, `SourceSummary` and `SourceReferenceId` are optional. A source reference must belong to the same Project; it may be a shared project reference or belong to any Requirement in that Project.

## External references

Controlled ReferenceTypes are `WebUrl`, `LocalFile`, `LocalDirectory`, `Document`, `Repository`, `GitHubRepository`, `GitHubIssue`, `GitHubPullRequest`, `ChatConversation`, `ExternalTask` and `SasdApplication`.

Every ExternalReference has a required ProjectId and an optional RequirementId. With no RequirementId it is a Project reference; otherwise it is a Requirement reference in the same Project. Polymorphic `OwnerType`/`OwnerId` is not used. Verification references follow the same same-Project rule.

## Target validation and opening

Target validation is centralized and testable. HTTPS is accepted for web, chat and external-task targets. File and directory types require absolute local paths. Document, Repository and SasdApplication use only the explicitly approved HTTPS/local variants. GitHub types require the matching GitHub HTTPS URL shape.

Credentials in URL userinfo, typical secret-bearing query parameters and reliably recognizable credential prefixes are rejected based only on the stored target string. Local files and external pages are never inspected for secrets.

Every open action validates again and then delegates to the normal operating-system handler. HTTPS targets are not fetched by PIMS. Missing local targets produce a controlled message and are never deleted automatically.

## Explicit non-goals

0.3.0 does not add Requirement relationships, attachments, document storage, task management, workflow configuration, provider authentication, provider APIs, automatic link checking, background checking or persisted reachability/health state.
