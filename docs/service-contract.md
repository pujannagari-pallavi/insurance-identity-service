# Identity Service Contract

## Ownership

Identity Service owns user identities, authentication, bearer-token issuance, and its database. Other services must not query its database.

## Public API

- Local URL: `http://localhost:5178`
- Gateway prefix: `/identity`
- Health endpoint: `GET /health`
- Development Swagger UI: `/swagger`

Service routes use the `/api/{resource}` convention. Error responses use RFC 7807 `application/problem+json` with `status`, `title`, and `detail`.

## Token Contract

Issued tokens use issuer `InsurancePlatform.Identity` and audience `InsurancePlatform.Clients`. They contain these claims:

| Claim | Meaning |
| --- | --- |
| `sub` and `nameidentifier` | Immutable identity user ID as a GUID |
| `email` | User email address |
| `name` | User name |
| `role` | Assigned role |
| `permission` | Explicit permission |

## Bootstrap Platform Administrator

To create the first platform administrator, configure these environment variables on the Identity Service before deployment:

```text
BootstrapAdmin__Email=admin@example.com
BootstrapAdmin__UserName=Platform Administrator
BootstrapAdmin__Password=<a strong unique password>
```

On startup, the service creates the user with only the `PlatformAdmin` role if the email does not exist. It never changes an existing user. Remove `BootstrapAdmin__Password` after the account is created; leaving incomplete bootstrap settings causes startup to fail.

## Administration API

Users with the `Identity.Users.Manage` permission can manage existing access assignments:

- `GET /api/administration/users`
- `GET /api/administration/roles`
- `PUT /api/administration/users/{userId}/roles` with `{ "roleIds": ["<role-guid>"] }`

## Events

Identity Service reserves `identity.user.registered.v1` for Customer Service consumers. Do not change its payload incompatibly without consumer review.

## Change Rules

Review the OpenAPI diff with API consumers before changing public endpoints. Keep secrets, database connection strings, and deployed service URLs outside source control.