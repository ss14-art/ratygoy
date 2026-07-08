using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Server.Xenoarchaeology.Equipment.Effects
{
    /// <summary>
    /// Конфигурация эффекта хаотичной трансформации предметов, адаптированная под архитектуру XAE.
    /// </summary>
    [RegisterComponent]
    public sealed partial class ChaosTransformArtifactComponent : Component
    {
        /// <summary>
        /// Радиус сбора предметов на полу и поиска инвентарей существ.
        /// </summary>
        [DataField("range")]
        public float Range = 12f;

        /// <summary>
        /// Какая доля предметов (от 0.0 до 1.0) будет превращена за одну активацию.
        /// Например, 0.2f означает, что превратится 20% от всех найденных вещей.
        /// </summary>
        [DataField("transformChance")]
        public float TransformChance = 0.2f;

        /// <summary>
        /// Черный список ID прототипов, в которые артефакту запрещено превращать вещи.
        /// </summary>
        [DataField("prototypeBlacklist")]
        public HashSet<EntProtoId>? PrototypeBlacklist;

        /// <summary>
        /// Черный список компонентов. Если у потенциального предмета есть хоть один компонент отсюда, он исключается.
        /// </summary>
        [DataField("componentBlacklist")]
        public HashSet<string>? ComponentBlacklist;

        /// <summary>
        /// Белый список компонентов. Предмет обязан содержать их все, чтобы артефакт мог в него превратить.
        /// </summary>
        [DataField("requiredComponents")]
        public HashSet<string>? RequiredComponents;
    }
}