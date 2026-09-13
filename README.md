# Rulealize.Plugin.Chance

Resolving what nobody chose, for [Rulealize](https://github.com/reny-develop/Rulealize) rule
sets.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Chance` |
| Namespace | `chance` |
| Reserved prefix | none |
| Depends on | `Rulealize.Abstraction` |
| Specification | [doc/specification.md](doc/specification.md) |

Two operations. `chance.pick` takes a sequence of things that could happen and a weight for
each, and produces one of them; `chance.roll` is a die of `faces` faces, which is the same
draw said in the one shape common enough to be worth naming.

```jsonc
// A card off a deck held as a count per rank: thirteen candidates, weighted by what is left
{ "op": "chance.pick",
  "of": { "op": "rec.keys", "of": "$deck" }, "as": "r",
  "weight": { "op": "rec.at", "record": "$deck", "key": "@r" } }
```

## It does not roll anything

There is no random number generator in this assembly, and that is the design rather than an
omission. `chance.pick` works out the candidates and their weights and hands them to the
runtime, which says which one this evaluation is for — because the runtime is what
enumerates the alternatives and reports where each one leads.

That is what makes the whole thing usable:

- **`GetOutcomes` walks the alternatives.** Every card that could come off the deck is a
  branch with a probability on it, so a search over a rule set with chance in it is the same
  two calls as a search over one without.
- **A transition stays reproducible.** An input document says what somebody decided and an
  outcome document says what the world did; the two together determine the next state, so a
  recorded pair replays to the state it was recorded against.
- **Guards and definitions keep their guarantees.** A value that read a clock would answer
  differently to each candidate `GetValidInputs` tries and would falsify the memoization
  definitions rely on.

Where the randomness ultimately comes from is a question for whoever is calling — sampling
one outcome by its probability is three lines above the runtime, and it is the caller that
knows whether this is a game, a simulation with a seed, or a replay of something that has
already happened.

## Where it may be written

Inside an input's `effects`, at any depth, and nowhere else. Not in a guard, a parameter
domain, an `actor`, `terminal`, or the body of a `definitions` entry. The runtime enforces
it when the rule set is compiled, with a JSON pointer to the offending node.

## What it needs from the host

A runtime that resolves draws — `Rulealize` 0.3.0 or later, on `Rulealize.Abstraction`
0.4.0 or later. An older one refuses the registration when this plugin is loaded, which is a
better moment to find out than when a rule set reaches for something the host cannot answer.

## Building

`dotnet build`. `Rulealize.Abstraction` restores from nuget.org like any other package, so
this repository builds on its own.

## License

Apache-2.0.
