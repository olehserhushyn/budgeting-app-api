# Product Requirements Document (PRD)

## 1. Overview

**Product Name:** Family Budgeting App  
**Tech Stack:**

- Backend: .NET (ASP.NET Core Web API)
- Frontend: React (TypeScript, Material UI, MobX)

**Purpose:**  
Enable users to manage personal and family finances by creating and tracking transactions (income, expenses, transfers), organizing budgets, planning spending/earning by categories, viewing statistics, and collaborating with others via shared budgets and ledgers.

---

## 2. User Stories / Use Cases

- **As a user, I want to:**
  1. Register, log in, and manage my account securely.
  2. Create, update, and delete transactions (income, expense, transfer; other types in future).
  3. Create and manage budgets, each with planned amounts for various categories.
  4. Organize categories and optional subcategories (e.g., groceries, salary, utilities) and assign planned amounts.
  5. View statistics and analytics for my budgets and transactions.
  6. Invite other users to collaborate on budgets or ledgers.
  7. Group multiple budgets under a ledger for higher-level financial management.
  8. Switch between different ledgers and budgets.
  9. See all budgets connected to a ledger.
  10. Manage currencies and transaction types.
  11. Receive email notifications for invitations and activity.
  12. Have different roles within a ledger (and eventually within a budget).
  13. Split expenses by shares, percentages, or custom amounts (future).
  14. Track who owes whom and settle up debts (future).
  15. Send payment requests or reminders for outstanding debts (future).
  16. Attach receipts/images to transactions (future/premium).
  17. Import expenses from CSV (e.g., Splitwise) and export data (future).
  18. Enter expenses in different currencies and convert as needed (future/premium).
  19. Get spending insights, trends, and smart suggestions (future/premium).
  20. Add expenses offline and sync later (future).
  21. Share with family, accountants, or others with granular permissions (future).
  22. Use the app on mobile devices (responsive web; future: dedicated app).

---

## 3. Core Features

### 3.1. Authentication & User Management

- User registration and login (JWT-based).
- Secure token handling.
- User invitations to budgets/ledgers.
- **User roles within ledgers** (e.g., admin, editor, viewer). _Planned: roles within budgets._

### 3.2. Ledgers

- Ledgers act as baskets containing multiple budgets and transactions outside of budgets.
- Create, view, and manage ledgers.
- Invite users to ledgers (collaborative budgeting).
- Assign roles to users within a ledger.
- View all budgets under a ledger.
- **User limits per ledger** (upgradeable with premium access).

### 3.3. Budgets

- Create, update, and delete budgets.
- Assign budgets to ledgers.
- View budget details, including planned and actual spending/earning.
- **User roles within budgets** (future feature).
- **User limits per budget** (upgradeable with premium access).

### 3.4. Categories & Subcategories

- Create, update, and delete categories.
- Subcategories are optional.
- Assign categories to budgets with planned amounts.
- Organize categories by transaction type (income, expense, transfer; other types in future).

### 3.5. Transactions

- Create, update, and delete transactions.
- Support for income, expense, and transfer types (other types in future).
- Assign transactions to budgets, categories, and ledgers.
- Allow transactions outside of budgets (ledger-level transactions).
- View transaction history and details.

### 3.6. Statistics & Analytics

- Overview and analysis tabs for budgets.
- Visual/statistical breakdowns by category, type, and time period.
- Export data (CSV, etc.) (future/premium).

### 3.7. Collaboration & Invitations

- Invite users to budgets or ledgers.
- Manage user roles and permissions within ledgers/budgets.
- **Email notifications for invitations and activity.**

### 3.8. Currencies & Transaction Types

- Support for multiple currencies.
- Manage transaction types (income, expense, transfer, etc.).
- **Multi-currency conversion** (future/premium).

### 3.9. Expense Splitting & Debt Tracking (Future)

- Split expenses by shares, percentages, or custom amounts.
- Track who owes whom and provide "settle up" suggestions.
- Send payment requests/reminders for outstanding debts.

### 3.10. Attachments & Import/Export (Future/Premium)

- Attach receipts/images to transactions.
- Import expenses from CSV (e.g., Splitwise) and export data.

### 3.11. Insights & Smart Suggestions (Future/Premium)

- Provide spending insights, trends, and smart suggestions.

### 3.12. Offline Mode (Future)

- Allow adding expenses offline and syncing later.

### 3.13. Access Control (Future)

- Granular permissions for sharing with family, accountants, etc.

### 3.14. Mobile Support

- Responsive web design.
- Dedicated mobile app (future).

### 3.15. Audit Logs (Final Stage)

- Track changes and actions for security and accountability.
- Viewable by users with appropriate permissions.

---

## 4. Non-Functional Requirements

- **Security:** JWT authentication, role-based access, secure data handling.
- **Performance:** Fast API responses, efficient state management in frontend.
- **Scalability:** Support for multiple users, ledgers, and budgets.
- **Usability:** Responsive UI, clear navigation, error handling.
- **Extensibility:** Modular codebase for adding new features (e.g., more analytics, integrations).
- **Notifications:** Email service integration for invitations and activity.

---

## 5. Out-of-Scope

- Direct bank integrations (unless specified).
- Advanced investment tracking.
- Mobile app (unless specified).
- Audit logs (until final stage).

---

## 6. Open Questions / Assumptions

- **User Roles:**
  - Roles are currently within ledgers; budget-level roles are planned.
  - What are the specific permissions for each role?
- **User Limits:**
  - What are the default and premium user limits per ledger/budget?
- **Notifications:**
  - What should the invitation email contain? Should there be reminders?
- **Audit Logs:**
  - What actions should be logged? Who can view them?
- **Premium Access:**
  - What features are gated behind premium (user limits, advanced analytics, attachments, etc.)?
- **Expense Splitting:**
  - What splitting methods should be supported (equal, shares, custom, etc.)?
- **Multi-currency:**
  - What conversion rates and sources should be used?
- **Mobile Support:**
  - Is a dedicated mobile app required, or is responsive web sufficient for now?

---

## 7. Appendix: Key API Endpoints & Frontend Flows

### Backend (Sample Endpoints)

- `/AuthController`: Register, login, token management.
- `/LedgerController`: CRUD for ledgers, invite users, assign roles.
- `/BudgetController`: CRUD for budgets, get budgets by ledger, get budget details.
- `/CategoryController`: CRUD for categories, assign to budgets.
- `/TransactionController`: CRUD for transactions, get transactions by budget/ledger, transfer.
- `/DashboardController`: Statistics and analytics.

### Frontend (Sample Pages/Flows)

- `LoginPage`, `RegisterPage`: Auth flows.
- `LedgersPage`: Manage/view ledgers, invite users, assign roles.
- `BudgetsPage`: List budgets, select ledger, create/manage budgets.
- `BudgetDetailsPage`: View/edit budget details, manage categories, see transactions, analytics.
- `TransactionsPage`: View and manage all transactions.
- `Dashboard`: Overview and statistics.

---

**If you have any specific requirements, features, or constraints not covered here, let me know and I'll update the PRD accordingly!**
