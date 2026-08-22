# Rulealize.Plugin.Chance

| | |
| --- | --- |
| Identifier | `Rulealize.Plugin.Chance` |
| Namespace | `chance` |
| Version | `1.0.0` |
| Reserved prefix | none |
| Depends on | [the value model](https://github.com/reny-develop/Rulealize.Abstraction/blob/main/doc/value-model.md), and nothing else |
| Requires | a runtime that resolves draws — `Rulealize.Abstraction` 0.4.0 or later, and a host built on it |
| Notation | [how a plugin specification is written](https://github.com/reny-develop/Rulealize.Abstraction/blob/main/doc/specification-notation.md) |

Resolving what nobody chose. One operation, and the shortest specification in the standard
set — almost all of it is about what the operation does *not* do.

**It does not choose.** `chance.pick` works out what could come out and how likely each of
those is, and asks the runtime for one of them. The runtime is what enumerates the
alternatives and reports where each one leads, so the runtime is what says which of them an
evaluation is for. Nothing in this plugin reads a clock or a random number generator, and a
rule set loading it does not become unpredictable.

**Which is what makes chance searchable.** `GetOutcomes` walks the branches a draw opens up
and puts a probability on each, so a search over a rule set with chance in it is the same
two calls in the same order as a search over one without. Sampling one of those branches is
the caller's job, three lines above the runtime, where it is known whether this is a game
being played, a simulation being run, or a hand being replayed.

## Nodes

| Node | Kind | |
| --- | --- | --- |
| `chance.pick` | draw | one of a sequence, weighted |

A **draw** is its own kind of operation. It builds an expression node like anything else
that produces a value, and what the kind settles is where it may be written.

---

## `chance.pick`

### Form

```jsonc
{
  "op": "chance.pick",
  "of": <expression:Sequence>,
  "as": "<name>",            // optional
  "weight": <expression:Number>   // optional
}
```

### How it evaluates

`of` is enumerated, and each element becomes a candidate. With a `weight`, the element is
bound by `as` and the weight is computed from it; without one, every candidate is equally
likely.

The candidates are handed to the runtime, which returns the one the current outcome is for.

### Weights are relative and are not normalized

Three cards of a rank left in a shoe is a weight of three. What fraction of the whole that
is depends on the other candidates, and the runtime is the one holding all of them.

**A weight of zero drops the candidate.** None of that rank left is an ordinary state of
affairs, not an error, and it is not an outcome of probability zero either — a caller
enumerating outcomes should not have to filter out things that cannot happen.

### Where it may be written

| | |
| --- | --- |
| may appear | anywhere inside `inputs.*.effects`, at any depth |
| refused in | `when`, `actor`, `params[].domain`, `terminal`, the body of a `definitions` entry |

The refusals are checked when the rule set is compiled, with a JSON pointer to the node.
Each of them is a position the runtime evaluates while it is sifting candidates or while it
is memoizing a result:

- **a guard** is evaluated once per candidate in a parameter's domain, and there is no
  outcome to be drawing for while the search is deciding what is legal
- **a domain** is enumerated to form those candidates, and `ApplyToState` walks it again to
  resolve an argument — a domain that drew would refuse the move it had just offered
- **`terminal`** is asked about a state, and whether a game is over is not a coin toss
- **a definition body** is memoized against its arguments and the snapshot, so a body that
  drew would answer its first caller and then repeat itself to every other one

### Evaluated twice is two draws

A draw happens each time control reaches the node. A `chance.pick` inside a projection over
three seats is three draws, with three entries in the outcome — which is what dealing a card
to each of three seats in one effect is asking for. It is not one value used three times.

### Faults

| | |
| --- | --- |
| `of` is not a `Sequence` | evaluation |
| `weight` is not a `Number` | evaluation |
| a weight is negative | evaluation. There is no reading of it |
| nothing could come out — `of` is empty, or every weight is zero | evaluation |
| a candidate has no canonical text | evaluation. A drawn value has to survive being written into an outcome document and read back, exactly as an input argument does |

**Nothing to draw from is a fault rather than an absence of outcomes.** A legal input has at
least one thing that can happen to it, and a caller relies on that: it is what lets a
traversal be written without a check for an empty result. A rule set that reaches an empty
deck is one whose guard forgot to say the deck is not empty, and the fault says so where it
happened rather than leaving a silent hole in a distribution.

---

## Decided

- **No `chance.int`, no `chance.roll`, no generator of any kind.** An operation that
  returned a number nobody enumerated would break four things at once: `GetValidInputs`
  would answer with one value and applying the input would produce another, a definition's
  memoized result would be a lie, `GetOutcomes` would have nothing to walk, and a recorded
  input would replay to a state it never produced. The last of those is the one that
  matters most — a state document and an input document determining the next state is the
  strongest property the library has, and this plugin exists because that property can be
  kept while still having chance in a rule set.
- **No seeded generator either, and it would not need to be here.** A pseudo-random sequence
  driven by a seed carried in the state is a pure function of its arguments and would fit
  the vocabulary without any of this. It is a different thing, though: it *resolves* chance
  rather than *enumerating* it, so `GetOutcomes` would report the one branch the seed picked
  and an expectation could not be computed. A plugin doing that is a reasonable thing to
  write and is not this one.
- **No continuous distributions.** A draw from a normal is not enumerable, and enumerability
  is the premise. There is no way to spell one here, which is better than a spelling that
  compiles and then cannot be searched.
- **`weight` rather than `probability`.** Weights compose without the rule set having to
  normalize anything, and the numbers a rule set has to hand are almost always counts. A
  probability would also have to be checked against a sum, which is a rule about a set of
  values that no single node is in a position to enforce.
- **`as` and `weight` rather than two parallel sequences.** The weight is nearly always
  computed from the candidate, and two sequences that had to stay the same length is exactly
  the kind of agreement that drifts. It is the same shape `seq.count` gives `where`.
- **One operation, and no `chance.shuffle`.** Shuffling a deck of fifty-two has 52! outcomes
  and none of them can be walked, so an operation producing one would be a generator by
  another name. A rule set that wants a shuffled deck models the deck as a count per rank
  and draws from it — which is the same distribution, thirteen candidates wide instead of
  unbounded, and searchable.
