# Requirements Document

## Introduction

Личный финансовый трекер — приложение для учёта доходов и расходов. Пользователь может добавлять транзакции (доход или расход), категоризировать их, просматривать историю и получать сводную аналитику по балансу и тратам за выбранный период. Цель — дать пользователю полное представление о своих финансах в одном месте.

## Glossary

- **Tracker** — основная система приложения, управляющая всеми данными и бизнес-логикой.
- **Transaction** — запись о финансовой операции: доход или расход с суммой, датой, категорией и описанием.
- **Category** — пользовательская или системная метка для классификации транзакций (например, «Еда», «Зарплата», «Транспорт»).
- **Balance** — текущий баланс: сумма всех доходов минус сумма всех расходов.
- **Period** — временной диапазон, заданный датой начала и датой окончания.
- **Report** — агрегированная сводка транзакций за выбранный Period.
- **User** — человек, использующий приложение.

---

## Requirements

### Requirement 1: Добавление транзакции

**User Story:** As a User, I want to add an income or expense transaction, so that I can keep a record of my financial operations.

#### Acceptance Criteria

1. WHEN the User submits a new transaction with a type (income or expense), amount, date, and category, THE Tracker SHALL save the transaction and assign it a unique identifier.
2. IF the User submits a transaction with a missing required field (type, amount, date, or category), THEN THE Tracker SHALL return a descriptive validation error and SHALL NOT save the transaction.
3. IF the User submits a transaction with an amount less than or equal to zero, THEN THE Tracker SHALL return a validation error stating that the amount must be a positive number.
4. IF the User submits a transaction with a date in an invalid format, THEN THE Tracker SHALL return a validation error specifying the expected date format.
5. WHERE the User provides an optional description, THE Tracker SHALL store the description alongside the transaction.

---

### Requirement 2: Редактирование транзакции

**User Story:** As a User, I want to edit an existing transaction, so that I can correct mistakes in previously entered data.

#### Acceptance Criteria

1. WHEN the User submits updated fields for an existing transaction, THE Tracker SHALL replace the previous values with the new values and preserve the transaction's unique identifier.
2. IF the User attempts to edit a transaction that does not exist, THEN THE Tracker SHALL return an error indicating that the transaction was not found.
3. IF the User submits an update with an invalid field value, THEN THE Tracker SHALL return a descriptive validation error and SHALL NOT apply the update.

---

### Requirement 3: Удаление транзакции

**User Story:** As a User, I want to delete a transaction, so that I can remove incorrectly entered records.

#### Acceptance Criteria

1. WHEN the User requests deletion of an existing transaction, THE Tracker SHALL permanently remove the transaction from storage.
2. IF the User requests deletion of a transaction that does not exist, THEN THE Tracker SHALL return an error indicating that the transaction was not found.

---

### Requirement 4: Просмотр истории транзакций

**User Story:** As a User, I want to view a list of my transactions, so that I can review my financial history.

#### Acceptance Criteria

1. WHEN the User requests the transaction list, THE Tracker SHALL return all transactions sorted by date in descending order.
2. WHEN the User requests the transaction list filtered by a Period, THE Tracker SHALL return only transactions whose date falls within the specified Period.
3. WHEN the User requests the transaction list filtered by a Category, THE Tracker SHALL return only transactions belonging to that Category.
4. WHEN the User requests the transaction list filtered by type (income or expense), THE Tracker SHALL return only transactions of the specified type.
5. WHEN the User requests the transaction list with a page size and page number, THE Tracker SHALL return the corresponding page of results and the total count of matching transactions.

---

### Requirement 5: Управление категориями

**User Story:** As a User, I want to create, rename, and delete categories, so that I can organise transactions according to my personal classification system.

#### Acceptance Criteria

1. WHEN the User creates a new Category with a unique name, THE Tracker SHALL save the Category and assign it a unique identifier.
2. IF the User creates a Category with a name that already exists, THEN THE Tracker SHALL return an error indicating that the Category name is already taken.
3. WHEN the User renames an existing Category, THE Tracker SHALL update the Category name and reflect the new name on all associated transactions.
4. IF the User attempts to delete a Category that has associated transactions, THEN THE Tracker SHALL return an error indicating that the Category cannot be deleted while transactions reference it.
5. WHEN the User deletes a Category that has no associated transactions, THE Tracker SHALL permanently remove the Category.

---

### Requirement 6: Просмотр текущего баланса

**User Story:** As a User, I want to see my current balance, so that I know how much money I have available.

#### Acceptance Criteria

1. WHEN the User requests the current Balance, THE Tracker SHALL calculate and return the sum of all income transactions minus the sum of all expense transactions.
2. WHEN the User requests the Balance for a specific Period, THE Tracker SHALL calculate and return the Balance using only transactions within that Period.

---

### Requirement 7: Аналитика и отчёты

**User Story:** As a User, I want to view a summary report for a selected period, so that I can understand my spending and income patterns.

#### Acceptance Criteria

1. WHEN the User requests a Report for a Period, THE Tracker SHALL return the total income, total expenses, and Balance for that Period.
2. WHEN the User requests a Report for a Period, THE Tracker SHALL return a breakdown of total expenses grouped by Category for that Period.
3. WHEN the User requests a Report for a Period, THE Tracker SHALL return a breakdown of total income grouped by Category for that Period.
4. IF the User requests a Report for a Period with no transactions, THEN THE Tracker SHALL return a Report with zero values for all fields.

---

### Requirement 8: Сериализация и хранение данных

**User Story:** As a User, I want my data to be persisted between sessions, so that I do not lose my financial history when I close the application.

#### Acceptance Criteria

1. THE Tracker SHALL persist all transactions and categories to durable storage so that data survives application restarts.
2. WHEN the Tracker loads data from storage, THE Tracker SHALL parse the stored representation into the internal data model.
3. THE Tracker SHALL serialise the internal data model into the storage representation before writing to storage.
4. FOR ALL valid data states, serialising then deserialising SHALL produce a data model equivalent to the original (round-trip property).
