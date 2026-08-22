// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Chance
{
    /// <summary>One of a sequence, weighted.</summary>
    /// <remarks>
    /// <para>
    /// The sequence is what could come out and <c>weight</c> is how likely each of those is
    /// beside the others. Both are ordinary expressions over the state snapshot, so what a
    /// draw can be is exactly what the rest of the vocabulary can compute — a deck held as a
    /// count per rank makes "which ranks are left" a <c>rec.keys</c> and "how many of this
    /// one" a <c>rec.at</c>, and the draw is thirteen wide with the right weights on it.
    /// </para>
    /// <para>
    /// <b>Nothing here chooses.</b> The candidates are handed to
    /// <see cref="IEvaluationContext.Draw"/> and the runtime says which of them this
    /// evaluation is for, because the runtime is what enumerates the alternatives and reports
    /// where each one leads. An operation that rolled its own number would answer differently
    /// to each candidate a search tries, would falsify the memoization definitions rely on,
    /// and would make a recorded transition replay to a state it never produced.
    /// </para>
    /// <para>
    /// The source is enumerated once per evaluation, and a run of the effects evaluates this
    /// node once per time control reaches it. A draw inside a projection that visits three
    /// elements is therefore three draws, not one drawn three times — which is what a rule
    /// set dealing a card to each of three seats in one effect is asking for, and what it
    /// gets.
    /// </para>
    /// </remarks>
    internal sealed class PickNode(ExpressionNode of, LocalSlot? element, ExpressionNode? weight) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // The source is built before the element name is declared, so a source cannot be
            // written in terms of the element it is about to produce.
            ExpressionNode source = context.RequireExpression("of");

            string? name = context.OptionalString("as");
            if (name is null)
            {
                return new PickNode(source, null, context.OptionalExpression("weight"));
            }

            using (context.Scope.BeginScope())
            {
                LocalSlot slot = context.Scope.Declare(name);
                return new PickNode(source, slot, context.OptionalExpression("weight"));
            }
        }

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            List<DrawCandidate> candidates = [];
            foreach (RuleValue value in of.Evaluate(context).AsSequence("chance.pick.of"))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (weight is null)
                {
                    candidates.Add(DrawCandidate.Even(value));
                    continue;
                }

                IEvaluationContext inner = element is LocalSlot slot ? context.Bind(slot, value) : context;
                candidates.Add(new DrawCandidate(value, weight.Evaluate(inner).AsNumber("chance.pick.weight")));
            }

            // Everything above is a pure function of the snapshot. The one thing that is not
            // is which of these comes back, and that belongs to whoever is enumerating them.
            return context.Draw(candidates.ToArray(), "chance.pick.of");
        }
    }
}
