# Deep Modules

From "A Philosophy of Software Design":

**Deep module** = small interface + lots of implementation

```
┌─────────────────────┐
│   Small Interface   │  ← Few methods on I{Module}ModuleApi
├─────────────────────┤
│                     │
│                     │
│  Deep Implementation│  ← Complex logic in handlers, domain entities
│                     │
│                     │
└─────────────────────┘
```

**Shallow module** = large interface + little implementation (avoid)

```
┌─────────────────────────────────┐
│       Large Interface           │  ← Many methods, pass-through DTOs
├─────────────────────────────────┤
│  Thin Implementation            │  ← Just maps and forwards
└─────────────────────────────────┘
```

When designing module APIs, ask:

- Can I reduce the number of methods on `I{Module}ModuleApi`?
- Can I simplify the command records (fewer properties)?
- Can I hide more complexity inside the handler/domain?
- Is this mapper doing anything beyond trivial property copying? If not, can the caller just construct the target directly?