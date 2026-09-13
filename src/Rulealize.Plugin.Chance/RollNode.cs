// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Chance
{
    /// <summary>A die: one of <c>faces</c> numbers, each as likely as the others.</summary>
    /// <remarks>
    /// <para>
    /// <c>chance.pick</c> over a sequence of the numbers says the same thing, and a rule set
    /// that wrote it that way was saying two things at once: what the alternatives are, and
    /// that they happen to be the first six integers. **A die is worth naming.** It is the
    /// commonest draw there is, it needs no sequence vocabulary to write, and a host reading
    /// the document can tell a die from an arbitrary draw — which is what lets one draw a die
    /// rather than a list of six things.
    /// </para>
    /// <para>
    /// Nothing here rolls anything, exactly as nothing in <c>chance.pick</c> picks: the faces
    /// are handed to the runtime as candidates, and which one this evaluation is for is the
    /// runtime's to say. Two rolls in one set of effects are two draws, and a rule set asking
    /// for 2d6 gets thirty-six outcomes with the right weights on them.
    /// </para>
    /// </remarks>
    internal sealed class RollNode(ExpressionNode faces) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return new RollNode(context.RequireExpression("faces"));
        }

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            int sides = faces.Evaluate(context).AsInt32("chance.roll.faces");

            if (sides < 1)
            {
                throw new RuleEvaluationException(
                    "chance.roll.faces",
                    $"A die has at least one face, but was asked for {sides}.");
            }

            DrawCandidate[] candidates = new DrawCandidate[sides];

            for (int face = 0; face < sides; face++)
            {
                candidates[face] = DrawCandidate.Even(RuleValue.Number(face + 1));
            }

            return context.Draw(candidates, "chance.roll.faces");
        }
    }
}
