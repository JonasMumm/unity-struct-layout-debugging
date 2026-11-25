using System;
using Unity.Assertions;
using Unity.Entities;
using UnityEngine;

namespace Unity.Physics
{
    public struct PhysicsOneWay : IComponentData, IEquatable<PhysicsOneWay>
    {
        public const int TypeSize = sizeof(ushort) + sizeof(ushort);

        ///only on the same layer can a one way collision happen. Layer == 0 means OneWayCollision Is not active,
        /// and any collision where at least one layer is = 0 is handled as a normal bidirectional collision
        ushort _oneWayLayerAndForceOverlayResultCreation;

        ///less InvPriority == "wins" collision and is considered kinematic in collision response.
        /// Higher InvPriority values get pushed out of the way.
        public ushort oneWayInvPriority;

        const ushort LastBit = (1 << 15);
        const ushort NotLastBit = ushort.MaxValue - LastBit;

        public ushort oneWayLayer
        {
            get => (ushort)(_oneWayLayerAndForceOverlayResultCreation & NotLastBit);
            set
            {
                UnityEngine.Assertions.Assert.IsTrue(value <= NotLastBit);
                _oneWayLayerAndForceOverlayResultCreation = (ushort)((ushort)(_oneWayLayerAndForceOverlayResultCreation & LastBit) | value);
            }
        }

        public bool forceOverlayResultCreationAcrossLayers
        {
            get => ((ushort)(_oneWayLayerAndForceOverlayResultCreation & LastBit)) == LastBit;
            set => _oneWayLayerAndForceOverlayResultCreation = (ushort)((ushort)(_oneWayLayerAndForceOverlayResultCreation & NotLastBit) | (ushort)(value ? LastBit : 0));
        }

        //TODO: use PackedBytes for onewWayLayer? 15 bit for layer, 1 bit for ForceOverlayResultCreation even when layer is not equal??

        public bool IsOneWayCollisionActive => oneWayLayer != 0;

        public bool Equals(PhysicsOneWay other)
        {
            return oneWayLayer == other.oneWayLayer && oneWayInvPriority == other.oneWayInvPriority;
        }

        public override bool Equals(object obj)
        {
            return obj is PhysicsOneWay other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(oneWayLayer, oneWayInvPriority);
        }

        public static bool IsCollisionEnabled(PhysicsOneWay caster, PhysicsOneWay receiverMustBeOlder, short casterPlaybackIndex, short receiverPlaybackIndex)
        {
            if (!receiverMustBeOlder.IsOneWayCollisionActive || !caster.IsOneWayCollisionActive) return true;
            return caster.oneWayLayer == receiverMustBeOlder.oneWayLayer
                && (caster.oneWayInvPriority > receiverMustBeOlder.oneWayInvPriority ||
                    (caster.oneWayInvPriority == receiverMustBeOlder.oneWayInvPriority
                        && casterPlaybackIndex < receiverPlaybackIndex));
        }

        public static bool ShouldCreateOneWayOverlayResult(PhysicsOneWay a, PhysicsOneWay b)
        {
            UnityEngine.Assertions.Assert.IsTrue(a.IsOneWayCollisionActive && b.IsOneWayCollisionActive);
            return a.oneWayLayer == b.oneWayLayer || a.forceOverlayResultCreationAcrossLayers || b.forceOverlayResultCreationAcrossLayers;
        }

        /// <summary>
        /// </summary>
        /// <returns>0 if equal, -1 if a wins, 1 if b wins.</returns>
        public static int Compare(PhysicsOneWay aOneWay, short aPlaybackIndex, PhysicsOneWay bOneWay, short bPlaybackIndex)
        {
            UnityEngine.Assertions.Assert.IsTrue(aOneWay.IsOneWayCollisionActive && bOneWay.IsOneWayCollisionActive);

            if (aOneWay.oneWayLayer != bOneWay.oneWayLayer) return 0;

            if (aOneWay.oneWayInvPriority != bOneWay.oneWayInvPriority)
            {
                return aOneWay.oneWayInvPriority < bOneWay.oneWayInvPriority
                    ? -1
                    : 1;
            }

            //Debug.Log("A "+aPlaybackIndex+" B "+bPlaybackIndex+"  prios " + aOneWay.oneWayInvPriorityy + "-" + bOneWay.oneWayInvPriorityy);

            return aPlaybackIndex < bPlaybackIndex
                ? 1
                : aPlaybackIndex == bPlaybackIndex
                    ? 0
                    : -1;
        }
    }
}