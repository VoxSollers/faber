# Third-party notices

Faber's original source code and project-authored assets are licensed under the
[Apache License 2.0](LICENSE). That license does not replace or relicense the
third-party material identified below. Each third-party component remains
subject to its upstream terms.

This inventory was verified against the upstream repositories on 2026-09-09.
`skills-lock.json` records source repositories and content hashes, but does not
record upstream commit revisions. The immutable links below therefore document
the upstream license state observed during this review; maintainers must repeat
the review whenever bundled skills are updated.

## Bundled agent skills

The canonical bundled copies live under `.agents/skills/`. Every entry under
`.claude/skills/` is a relative symbolic-link alias to its matching bundled skill
directory, rather than a redistributed second copy. The
`scripts/verify-third-party-notices.sh` guard enforces this structural invariant.

| Upstream source | Bundled skill directories | Terms and attribution | Publication status |
|---|---|---|---|
| [aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills) | `api-design`, `aspire-configuration`, `aspire-integration-testing`, `aspire-service-defaults`, `dependency-injection-patterns`, `efcore-patterns`, `mailpit-integration`, `microsoft-extensions-configuration`, `modern-csharp-coding-standards`, `opentelemetry-net-instrumentation`, `testcontainers-integration-tests` | [MIT](https://github.com/Aaronontheweb/dotnet-skills/blob/13e26d39ed01d97ea592235d041304d289f4ba07/LICENSE), Copyright (c) 2025 Aaron Stannard | Allowed with the MIT notice retained below. |
| [analogjs/angular-skills](https://github.com/analogjs/angular-skills) | `angular-component`, `angular-di`, `angular-forms`, `angular-http`, `angular-routing`, `angular-signals`, `angular-testing`, `angular-tooling` | [MIT](https://github.com/analogjs/angular-skills/blob/18996fd1b2bf663c978065a8d5417393e72ccc6a/LICENSE), Copyright (c) 2026 Brandon Roberts | Allowed with the MIT notice retained below. |
| [codewithmukesh/dotnet-claude-kit](https://github.com/codewithmukesh/dotnet-claude-kit) | `vertical-slice` | [MIT](https://github.com/codewithmukesh/dotnet-claude-kit/blob/23300897f4d1a276bc475e373ecfe886e43dd7c5/LICENSE), Copyright (c) 2025 Mukesh Murugan | Allowed with the MIT notice retained below. |
| [dotnet/skills](https://github.com/dotnet/skills) | `configuring-opentelemetry-dotnet`, `migrate-xunit-to-xunit-v3`, `optimizing-ef-core-queries` | [MIT](https://github.com/dotnet/skills/blob/c4a3f7ad4fd8fb50c02a42a6375c0aba4f92e9f7/LICENSE), Copyright (c) .NET Foundation and Contributors | Allowed with the MIT notice retained below. |
| [github/awesome-copilot](https://github.com/github/awesome-copilot) | `aspire`, `csharp-docs` | [MIT](https://github.com/github/awesome-copilot/blob/7568a482ce2df38f8965ab5336a3220db796a4ba/LICENSE), Copyright GitHub, Inc. | Allowed with the MIT notice retained below. |
| [kevintsengtw/dotnet-testing-agent-skills](https://github.com/kevintsengtw/dotnet-testing-agent-skills) | `dotnet-testing-advanced-aspire-testing`, `dotnet-testing-advanced-testcontainers-database`, `dotnet-testing-advanced-xunit-upgrade-guide`, `dotnet-testing-bogus-fake-data`, `dotnet-testing-fluentvalidation-testing`, `dotnet-testing-nsubstitute-mocking`, `dotnet-testing-test-data-builder-pattern`, `dotnet-testing-test-naming-conventions` | [MIT](https://github.com/kevintsengtw/dotnet-testing-agent-skills/blob/4e34a9776431f3f189ae8ec72840a415bfaad0e0/LICENSE), Copyright (c) 2026 Kevin Tseng | Allowed with the MIT notice retained below. |
| [mattpocock/skills](https://github.com/mattpocock/skills) | `tdd` | [MIT](https://github.com/mattpocock/skills/blob/3cca18b368ae95cdbdebbff572ccafa662551015/LICENSE), Copyright (c) 2026 Matt Pocock | Allowed with the MIT notice retained below. |
| [vercel-labs/skills](https://github.com/vercel-labs/skills) | `find-skills` | [MIT](https://github.com/vercel-labs/skills/blob/80feb48868972d518436f26711509bc78595b5cb/LICENSE), Copyright (c) 2026 Vercel, Inc. | Allowed with the MIT notice retained below. |
| [wshobson/agents](https://github.com/wshobson/agents) | `cqrs-implementation`, `dotnet-backend-patterns`, `tailwind-design-system` | [MIT](https://github.com/wshobson/agents/blob/a30778f8c4e6b0a87567941b7cca4f534bf642b6/LICENSE), Copyright (c) 2024 Seth Hobson | Allowed with the MIT notice retained below. |

Every unique source in `skills-lock.json` is represented in the table above.
`scripts/verify-third-party-notices.sh` verifies the source sets agree.

## Web and design assets

- `src/web/faber-app/public/favicon.ico` is byte-for-byte identical to the
  [Angular CLI application template favicon](https://github.com/angular/angular-cli/blob/0ba4d8e29c0fb9abe6b07432edfab9e39066ca29/packages/schematics/angular/application/files/common-files/public/favicon.ico.template).
  It is distributed under the [MIT License](https://github.com/angular/angular-cli/blob/0ba4d8e29c0fb9abe6b07432edfab9e39066ca29/LICENSE),
  Copyright (c) 2010-2026 Google LLC.
- `src/web/faber-app/public/flags/gb.svg` and `pl.svg` were added as original,
  minimal vector drawings in Faber commit `e452f1b9` and have no embedded or
  linked third-party content. They are project-authored assets covered by
  Faber's Apache-2.0 license to the extent copyright applies. Country names and
  flag designs may also be subject to laws unrelated to copyright in some
  jurisdictions.
- `src/web/faber-app/design.pen` was added as a project design source in Faber
  commit `33add24e`. It contains JSON design primitives, project text and color
  values, font-family names, and references by name to Lucide icons. It embeds
  no font files, icon glyph data, raster images, external URLs, or other binary
  assets. The file itself is project-authored and covered by Faber's
  Apache-2.0 license.

## MIT license text

The following notice applies to the MIT-licensed material identified above,
with the applicable copyright holder substituted from the relevant row.

> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in
> all copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.
