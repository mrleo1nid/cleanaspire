---
name: mcp-tools
description: Always check for and use MCP tools when available. DeepWiki for open-source repositories, Context7 for library documentation, Microsoft Learn for Microsoft technologies.

---

# MCP Tools Priority Guide

**Always use MCP tools when available** before falling back to other methods.

## DeepWiki - Open Source Repositories
Use `DeepWiki_ask_question` for:
- Bit.BlazorUI: repository `bitfoundation/bitplatform`
- Mapperly: repository `riok/mapperly`  
- Keycloak: repository `keycloak/keycloak`

## Context7 - Current Library Docs
1. First: `mcp_context7_resolve-library-id`
2. Then: `mcp_context7_query-docs` with resolved ID
For: NuGet packages, current version-specific documentation.

## Microsoft Learn - Microsoft Tech
Use available functions for:
- **microsoft_docs_search** - Semantic search of Microsoft technical documentation  
  Parameters: `query` (string) - search query
- **microsoft_docs_fetch** - Fetch and convert Microsoft docs pages to Markdown
  Parameters: `url` (string) - documentation page URL
- **microsoft_code_sample_search** - Find official Microsoft/Azure code samples
  Parameters: `query` (string) - code search query, `language` (string, optional) - programming language filter

For: .NET, ASP.NET Core, EF Core, Azure, official Microsoft documentation.