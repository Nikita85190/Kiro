# Requirements Document

## Introduction

This feature extends the Expense Tracker's Transaction entity with richer annotation capabilities. Currently a transaction carries an optional plain-text `description` field. This feature adds:

1. **Notes** — a longer, free-form text field that replaces/extends the existing description, allowing users to record context, receipts references, or any narrative about a transaction.
2. **Attachments** — the ability to upload one or more files (e.g. receipt images, PDF invoices) and associate them with a transaction, and later download or remove them.

The backend is an in-memory ASP.NET Core 8 Web API; files are stored on the local filesystem (configurable base path) rather than a database. The frontend is React 19 + TypeScript.

---

## Glossary

- **Transaction**: A financial event (income or expense) already tracked by the system, identified by a UUID.
- **Note**: A free-form text annotation attached to a Transaction, up to 2 000 characters.
- **Attachment**: A binary file uploaded by the user and associated with a Transaction.
- **Attachment_Store**: The server-side component responsible for persisting and retrieving attachment files.
- **Attachment_Repository**: The in-memory index that maps attachment metadata (id, filename, content type, size, transaction id) to stored files.
- **Transaction_Service**: The existing service layer that owns business logic for transactions.
- **Attachment_Service**: The new service layer responsible for attachment upload, retrieval, and deletion business logic.
- **Transactions_Controller**: The existing ASP.NET Core controller at `api/transactions`.
- **Attachments_Controller**: The new ASP.NET Core controller at `api/transactions/{transactionId}/attachments`.
- **Transaction_Form**: The React component used to create and edit transactions.
- **Transaction_Detail**: The React component (new) that displays a single transaction's full details, note, and attachments.

---

## Requirements

### Requirement 1: Extended Note on a Transaction

**User Story:** As a user, I want to add a detailed note to a transaction, so that I can record context or reminders beyond a short description.

#### Acceptance Criteria

1. THE Transaction_Service SHALL accept a `note` field (string, up to 2 000 characters) on create and update operations, in addition to the existing `description` field.
2. WHEN a create or update request includes a `note` value exceeding 2 000 characters, THE Transaction_Service SHALL reject the request with a validation error identifying the `note` field.
3. WHEN a create or update request omits the `note` field, THE Transaction_Service SHALL store the transaction with a null note.
4. THE Transaction_Service SHALL include the `note` value in every `TransactionResponse` returned to callers.
5. WHEN a transaction is updated and the `note` field is set to an empty string, THE Transaction_Service SHALL store null for the note (treating empty as absent).

---

### Requirement 2: Attach Files to a Transaction

**User Story:** As a user, I want to upload files (receipts, invoices) to a transaction, so that I have documentary evidence linked directly to that financial record.

#### Acceptance Criteria

1. WHEN a user submits a file upload request for a valid transaction id, THE Attachment_Service SHALL store the file and return an `AttachmentResponse` containing the attachment id, original filename, content type, file size in bytes, and upload timestamp.
2. WHEN a file upload request references a transaction id that does not exist, THE Attachment_Service SHALL throw a `NotFoundException`.
3. WHEN a file upload request includes a file whose size exceeds 10 MB, THE Attachment_Service SHALL reject the request with a validation error specifying the size limit.
4. WHEN a file upload request includes a file with a content type not in the permitted set (image/jpeg, image/png, image/gif, image/webp, application/pdf), THE Attachment_Service SHALL reject the request with a validation error listing the permitted types.
5. WHEN a transaction already has 10 attachments, THE Attachment_Service SHALL reject any further upload for that transaction with a `BusinessRuleException` stating the per-transaction attachment limit.
6. THE Attachment_Service SHALL assign each attachment a unique UUID identifier upon successful upload.

---

### Requirement 3: Retrieve Attachments for a Transaction

**User Story:** As a user, I want to see all files attached to a transaction, so that I can review supporting documents.

#### Acceptance Criteria

1. WHEN a request is made to list attachments for a valid transaction id, THE Attachment_Service SHALL return all `AttachmentResponse` records associated with that transaction, ordered by upload timestamp ascending.
2. WHEN a request is made to list attachments for a transaction id that does not exist, THE Attachment_Service SHALL throw a `NotFoundException`.
3. WHEN a transaction has no attachments, THE Attachment_Service SHALL return an empty list.
4. THE Attachments_Controller SHALL expose a `GET api/transactions/{transactionId}/attachments` endpoint that returns the list from the Attachment_Service.

---

### Requirement 4: Download an Attachment

**User Story:** As a user, I want to download an attachment, so that I can view the original file.

#### Acceptance Criteria

1. WHEN a download request is made for an attachment id that exists and belongs to the specified transaction, THE Attachment_Service SHALL return the file content together with its original filename and content type.
2. WHEN a download request references an attachment id that does not exist, THE Attachment_Service SHALL throw a `NotFoundException`.
3. WHEN a download request references an attachment id that belongs to a different transaction than the one in the URL, THE Attachment_Service SHALL throw a `NotFoundException`.
4. THE Attachments_Controller SHALL expose a `GET api/transactions/{transactionId}/attachments/{attachmentId}` endpoint that streams the file content with the correct `Content-Type` and `Content-Disposition: attachment` headers.

---

### Requirement 5: Delete an Attachment

**User Story:** As a user, I want to remove an attachment from a transaction, so that I can correct mistakes or free up storage.

#### Acceptance Criteria

1. WHEN a delete request is made for an attachment id that exists and belongs to the specified transaction, THE Attachment_Service SHALL remove the file from the Attachment_Store and remove the metadata from the Attachment_Repository.
2. WHEN a delete request references an attachment id that does not exist, THE Attachment_Service SHALL throw a `NotFoundException`.
3. WHEN a delete request references an attachment id that belongs to a different transaction than the one in the URL, THE Attachment_Service SHALL throw a `NotFoundException`.
4. WHEN a transaction is deleted, THE Attachment_Service SHALL delete all attachments associated with that transaction.
5. THE Attachments_Controller SHALL expose a `DELETE api/transactions/{transactionId}/attachments/{attachmentId}` endpoint that returns HTTP 204 on success.

---

### Requirement 6: Transaction Response Includes Attachment Metadata

**User Story:** As a user, I want the transaction list and detail views to show how many attachments a transaction has, so that I know at a glance whether documents are on file.

#### Acceptance Criteria

1. THE Transaction_Service SHALL include an `attachmentCount` integer field in every `TransactionResponse`, reflecting the current number of attachments for that transaction.
2. WHEN a transaction has no attachments, THE Transaction_Service SHALL return `attachmentCount` as 0.
3. WHEN an attachment is added or deleted, THE Transaction_Service SHALL reflect the updated count in subsequent `TransactionResponse` results for that transaction.

---

### Requirement 7: Frontend — Note Field in Transaction Form

**User Story:** As a user, I want to enter a note when creating or editing a transaction, so that I can capture details at the time of entry.

#### Acceptance Criteria

1. THE Transaction_Form SHALL render a multi-line text area labelled "Note" below the existing description field.
2. WHEN the note text area contains more than 2 000 characters, THE Transaction_Form SHALL display an inline validation error and prevent form submission.
3. WHEN the Transaction_Form is opened in edit mode, THE Transaction_Form SHALL pre-populate the note text area with the transaction's existing note value.
4. WHEN the Transaction_Form is submitted with a valid note, THE Transaction_Form SHALL include the note value in the API request payload.

---

### Requirement 8: Frontend — Attachment Upload in Transaction Detail

**User Story:** As a user, I want to upload attachments from the transaction detail view, so that I can associate files without leaving the context of the transaction.

#### Acceptance Criteria

1. THE Transaction_Detail SHALL render a file input that accepts image/jpeg, image/png, image/gif, image/webp, and application/pdf files.
2. WHEN a user selects a file that exceeds 10 MB, THE Transaction_Detail SHALL display an error message before sending any request to the server.
3. WHEN a file upload succeeds, THE Transaction_Detail SHALL add the new attachment to the displayed attachment list without requiring a full page reload.
4. WHEN a file upload fails due to a server error, THE Transaction_Detail SHALL display the error message returned by the API.
5. WHILE a file upload is in progress, THE Transaction_Detail SHALL display a loading indicator and disable the upload control.

---

### Requirement 9: Frontend — Attachment List and Download

**User Story:** As a user, I want to see and download attachments from the transaction detail view, so that I can access my documents.

#### Acceptance Criteria

1. THE Transaction_Detail SHALL display each attachment as a row showing the original filename, file size in human-readable form (e.g. "1.2 MB"), and upload date.
2. WHEN a user activates the download action for an attachment, THE Transaction_Detail SHALL initiate a browser file download using the attachment's original filename.
3. WHEN a transaction has no attachments, THE Transaction_Detail SHALL display a message indicating no attachments are present.

---

### Requirement 10: Frontend — Delete Attachment

**User Story:** As a user, I want to remove an attachment from the transaction detail view, so that I can manage my files.

#### Acceptance Criteria

1. THE Transaction_Detail SHALL render a delete control for each attachment in the list.
2. WHEN a user activates the delete control for an attachment, THE Transaction_Detail SHALL request confirmation before sending the delete request.
3. WHEN the delete request succeeds, THE Transaction_Detail SHALL remove the attachment from the displayed list without requiring a full page reload.
4. WHEN the delete request fails, THE Transaction_Detail SHALL display the error message returned by the API.

---

### Requirement 11: Attachment Storage Configuration

**User Story:** As a developer, I want the attachment storage path to be configurable, so that I can control where files are written without changing code.

#### Acceptance Criteria

1. THE Attachment_Store SHALL read the storage base path from the application configuration key `Attachments:StoragePath`.
2. IF the configured storage path does not exist at application startup, THEN THE Attachment_Store SHALL create the directory.
3. IF the `Attachments:StoragePath` configuration key is absent, THEN THE Attachment_Store SHALL use a default path of `attachments/` relative to the application's working directory.

---

### Requirement 12: Attachment Round-Trip Integrity

**User Story:** As a user, I want the files I upload to be returned exactly as uploaded, so that my documents are not corrupted.

#### Acceptance Criteria

1. FOR ALL valid files uploaded via the Attachment_Service, downloading the same attachment SHALL return byte-for-byte identical content to what was uploaded (round-trip property).
2. THE Attachment_Service SHALL preserve the original filename as provided by the client on upload and return it unchanged on download.
