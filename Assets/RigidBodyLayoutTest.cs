using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using static Unity.Physics.Math;

namespace Default
{
    /// <summary>   An instance of a collider in a physics world.
    /// Using StructLayout to handle for an issue where there can be a mismatch
    /// between struct sizes for RigidBody in separate jobs
    /// THIS IS A COPY OF ALL MEMBERS OF Unity.Physics.Rigidbody, WITH ONLY THE StructLayoutAttribute CHANGED
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 1, Size = 56)]
    public struct RigidBodyLayoutTest
    {
        /// <summary>   The rigid body's collider (allowed to be null) </summary>
        [System.Runtime.InteropServices.FieldOffset(0)]
        public BlobAssetReference<Collider> Collider;

        /// <summary>   The rigid body's transform in world space. </summary>
        [System.Runtime.InteropServices.FieldOffset(8)]
        public RigidTransform WorldFromBody;

        /// <summary>   The entity that rigid body represents. </summary>
        [System.Runtime.InteropServices.FieldOffset(36)]
        public Entity Entity;

        /// <summary>
        /// Arbitrary custom tags. These get copied into contact manifolds and can be used to inform
        /// contact modifiers.
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(44)]
        public byte CustomTags;

        /// <summary>
        /// <para> The rigid body's <see cref="Unity.Physics.SolverType">solver type</see>.</para>
        /// <para> Used for determining which type of solver should be used to resolve collisions between this
        /// and other bodies.</para>
        /// <para> Default is <see cref="SolverType.Iterative">iterative</see>.</para>
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(45)]
        public SolverType SolverType;

        /// <summary>   The rigid body's scale in world space. </summary>
        [System.Runtime.InteropServices.FieldOffset(48)]
        public float Scale;

        /// <summary>   Rigid body that thas identity transform, null entity, and a null collider. </summary>
        public static readonly RigidBody Zero = new RigidBody
        {
            WorldFromBody = RigidTransform.identity,
            Collider = default,
            Entity = Entity.Null,
            CustomTags = 0,
            Scale = 1.0f
        };
    }
}
