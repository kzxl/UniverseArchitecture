# Security & Threat Model
**Project**: Universe Architecture v3.0

## 1. Security Goal
Prevent the domino effect: "A failing or compromised module must not impact other modules or the core registry."

## 2. Attack Surface
- Dispatching unauthorized commands to sensitive modules.
- Eavesdropping or spoofing events on the Event Bus.
- Bypassing the Middleware.

## 3. Sandboxing & Access Control List (ACL)
Universe Architecture introduces `ISecureModule`, requiring modules to declare their Security Policies.
An `AccessControlMiddleware` (a Gravity handler) checks if the caller (`Principal`) possesses the `RequiredClaims` for the target module. This blocks unauthorized execution at the perimeter.

Furthermore, **Nested Registries** allow inserting **Local Middlewares**, establishing Secure Enclaves (e.g., all `hr.*` commands require HR permissions, regardless of outer network status).

## 4. Data Sovereignty
Rule #6: "Every galaxy has its own star system."
Modules must **never** reference the Database Context of another Module.
By enforcing physical DB schema separation, SQL Injection or data leaks in Module A are strictly contained, protecting the rest of the application.
