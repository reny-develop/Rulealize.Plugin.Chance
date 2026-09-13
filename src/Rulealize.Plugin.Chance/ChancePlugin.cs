// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Plugin;

namespace Rulealize.Plugin.Chance
{
    /// <summary>
    /// Resolving what nobody chose, in the <c>chance</c> namespace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two operations, and neither generates anything. <c>chance.pick</c> works out what
    /// could come out and how likely each of those is, and then asks the runtime for one of
    /// them — which is the whole of the arrangement; <c>chance.roll</c> is the same draw
    /// over the faces of a die, which is the one shape common enough to be worth naming.
    /// There is no random number generator in this assembly and there is not meant to be
    /// one: the runtime is what enumerates the alternatives, so the runtime is what says
    /// which of them an evaluation is for, and where the randomness ultimately comes from
    /// is a question for whoever is calling.
    /// </para>
    /// <para>
    /// That is also why this registers with <c>AddDraw</c> rather than <c>AddExpression</c>.
    /// What it builds is an expression node like anything else that produces a value; what
    /// the registration says is that it may be written inside an input's <c>effects</c> and
    /// nowhere else. Guards, parameter domains, actors, <c>terminal</c> and definition bodies
    /// are all evaluated while candidates are being sifted or while a result is being
    /// memoized, and none of that survives a value the state snapshot does not settle.
    /// </para>
    /// <para>
    /// A runtime with no notion of a draw refuses the registration when this is loaded, which
    /// is the right moment for it: better than a rule set finding out it reached for
    /// something the host cannot resolve.
    /// </para>
    /// </remarks>
    public sealed class ChancePlugin : IRulealizePlugin
    {
        /// <inheritdoc />
        public PluginManifest Manifest { get; } =
            new("Rulealize.Plugin.Chance", new Version(1, 1, 0), "chance");

        /// <inheritdoc />
        public void Register(IPluginRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);

            registry.AddDraw("pick", PickNode.Build);
            registry.AddDraw("roll", RollNode.Build);
        }
    }
}
