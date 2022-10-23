# Demo of a multi-tenant application using Grpc, Dapper and Postgres with Row level security

## GRPC Request Pipeline

### ExceptionInterceptor
Trap for exceptions and translate them to GRPC response status codes

### ValidationInterceptor 
Finds a validator for the GRPC request and uses it to validate the request or throw a validation exception

### TenantContextInterceptor
Extracts the tenant identifier from the GRPC request and stores it in the tenant context.

## Mediatr Request Pipeline

### LoggingBehaviour 
Log the request being executed

### TransactionBehaviour 
Open a database connection and begin a transaction

### TenantContextBehaviour 
Retrieves the tenant identity from the tenant context then uses the open connection to set tenant context in db

## Create a table for use by multiple tenants

```cs
Create.Table("cars")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("tenant").AsString().NotNullable() // This column indicates which tenant a row belongs to
            .WithColumn("registration").AsString().Nullable().Unique()
            .WithColumn("data").AsCustom("jsonb").NotNullable();
```   

## Configuring security policy on the table

```sql
// Create a separate account for tenants to login with
Execute.Sql($"CREATE USER {Username} LOGIN PASSWORD '{Password}';");

// Give this tenant account access to the table 
Execute.Sql($"GRANT SELECT, UPDATE, INSERT, DELETE ON {Table} TO {Username};");

// This table should have row level security that ensure a tenant can only manage their own data
Execute.Sql($"ALTER TABLE {Table} ENABLE ROW LEVEL SECURITY;");

// Define the policy that will be applied
Execute.Sql($"CREATE POLICY {Policy} ON {Table} FOR ALL TO {Username} USING (tenant = current_setting('app.tenant')::VARCHAR);");
```

## Interacting with security policy

```sql
// The the tenant context for this connection
SET app.tenant = '{Tenant}'

// Only this rows belonging to this tenant will be returned
SELECT * FROM {Table}
```
