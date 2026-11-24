using System;
using System.Runtime.InteropServices;
using Default;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Collider = Unity.Physics.Collider;

[BurstCompile]
public class StructSizeTest
{
    struct Bodies
    {
        public NativeArray<RigidBody> unityBodies;
        public NativeArray<RigidBodyLayoutTest> testBodies;
    }

    static void RunTestManaged(in Bodies bodies)
    {
        LogErrorIfBurst();
        DoTest(bodies);
    }

    [BurstCompile] //Another Issue: UnityEditor.MenuItem-methods are never burst-executed, but burst-compatible methods within are.
    static void RunTestBurst(in Bodies bodies)
    {
        LogErrorIfNoBurst();
        DoTest(bodies);
    }

    [UnityEditor.MenuItem("Test/Run All Tests")]
    public static void RunAllTests()
    {
        var prevLogType = Application.GetStackTraceLogType(LogType.Log);
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        var collider = CreateColliderBlob();
        var bodiesManaged = CreateBodiesManaged(collider);
        //Bodies bodiesBurst = default;
        CreateBodiesBurst(collider, out var bodiesBurst);
        Debug.Log("--- Running Test Managed with managed-created bodies: ---");
        RunTestManaged(bodiesManaged);
        Debug.Log("--- Running Test Burst with Burst-created bodies: ---");
        RunTestBurst(bodiesBurst);

        Debug.Log("--- Running Test Managed with Burst-created bodies: ---");
        RunTestManaged(bodiesBurst);
        Debug.Log("--- Running Test Burst with managed-created bodies: ---");
        RunTestBurst(bodiesManaged);

        Application.SetStackTraceLogType(LogType.Log, prevLogType);
    }

    [BurstDiscard]
    static void LogErrorIfNoBurst()
    {
        Debug.LogError("This is not bursted but should be.");
    }

    static void LogErrorIfBurst()
    {
        if (BurstUtils.IsBurstCompiled())
        {
            Debug.LogError("This is bursted and shouldn't be.");
        }
    }

    static unsafe void HandleUnityBody(NativeArray<RigidBody> bodies, int index)
    {
        ref var body = ref UnsafeUtility.ArrayElementAsRef<RigidBody>(bodies.GetUnsafeReadOnlyPtr(), index);
        var size = sizeof(RigidBody);
        var basePtr = UnsafeUtility.AddressOf(ref body);
        var colliderFieldPtr = UnsafeUtility.AddressOf(ref body.Collider);
        var scaleFieldPtr = UnsafeUtility.AddressOf(ref body.Scale);

        var basePtrLong = PtrAsLong(basePtr);
        var colliderFieldPtrLong = PtrAsLong(colliderFieldPtr);
        var scaleFieldPtrLong = PtrAsLong(scaleFieldPtr);
        var colliderFieldOffsetLong = colliderFieldPtrLong - basePtrLong;
        var scaleFieldOffsetLong = scaleFieldPtrLong - basePtrLong;
        var isBurst = BurstUtils.IsBurstCompiled();
        var offsetToArrayStart = basePtrLong - PtrAsLong(bodies.GetUnsafeReadOnlyPtr());

        Debug.Log($"Unity[{index}]: Size: {size}, BaseAddress: {basePtrLong}, OffsetFromArray: {offsetToArrayStart}, ColliderFieldOffset: {colliderFieldOffsetLong}, ScaleFieldOffset: {scaleFieldOffsetLong}, ScaleValue: {body.Scale}, is Burst: {isBurst}");
    }

    static unsafe void HandleTestBody(NativeArray<RigidBodyLayoutTest> bodies, int index)
    {
        ref var body = ref UnsafeUtility.ArrayElementAsRef<RigidBodyLayoutTest>(bodies.GetUnsafeReadOnlyPtr(), index);
        var size = sizeof(RigidBody);
        var basePtr = UnsafeUtility.AddressOf(ref body);
        var colliderFieldPtr = UnsafeUtility.AddressOf(ref body.Collider);
        var scaleFieldPtr = UnsafeUtility.AddressOf(ref body.Scale);

        var basePtrLong = PtrAsLong(basePtr);
        var colliderFieldPtrLong = PtrAsLong(colliderFieldPtr);
        var scaleFieldPtrLong = PtrAsLong(scaleFieldPtr);
        var colliderFieldOffsetLong = colliderFieldPtrLong - basePtrLong;
        var scaleFieldOffsetLong = scaleFieldPtrLong - basePtrLong;
        var isBurst = BurstUtils.IsBurstCompiled();
        var offsetToArrayStart = basePtrLong - PtrAsLong(bodies.GetUnsafeReadOnlyPtr());

        Debug.Log($"Test [{index}]: Size: {size}, BaseAddress: {basePtrLong}, OffsetFromArray: {offsetToArrayStart}, ColliderFieldOffset: {colliderFieldOffsetLong}, ScaleFieldOffset: {scaleFieldOffsetLong}, ScaleValue: {body.Scale}, is Burst: {isBurst}");
    }

    static void DoTest(Bodies bodies)
    {
        //unity
        HandleUnityBody(bodies.unityBodies, 0);
        HandleUnityBody(bodies.unityBodies, 1);

        //testBody
        HandleTestBody(bodies.testBodies, 0);
        HandleTestBody(bodies.testBodies, 1);
    }

    static ulong PtrAsLong(IntPtr ptr)
    {
        return UnsafeUtility.As<IntPtr, ulong>(ref ptr);
    }

    static unsafe ulong PtrAsLong(void* ptr)
    {
        return PtrAsLong((IntPtr)ptr);
    }

    static BlobAssetReference<Collider> CreateColliderBlob()
    {
        var blobAssetBuilder = new BlobBuilder(Allocator.Temp);
        ref var collideRoot = ref blobAssetBuilder.ConstructRoot<Collider>();
        var collider = blobAssetBuilder.CreateBlobAssetReference<Collider>(Allocator.Persistent);
        return collider;
    }

    [BurstCompile]
    static void CreateBodiesBurst(in BlobAssetReference<Collider> collider, out Bodies bodies)
    {
        LogErrorIfNoBurst();
        bodies = CreateBodies(collider);
    }
    
    static Bodies CreateBodiesManaged(BlobAssetReference<Collider> collider)
    {
        LogErrorIfBurst();
        return CreateBodies(collider);
    }

    static Bodies CreateBodies(BlobAssetReference<Collider> collider)
    {
        var worldFromBody = new RigidTransform(quaternion.identity, new float3(1f, 2f, 3f));
        var entity = new Entity
        {
            Index = 1,
            Version = 2,
        };

        var tags = byte.MaxValue;
        var solverType = SolverType.Direct;
        var scale = 5f;

        var unityRigidBody = new RigidBody
        {
            Collider = collider,
            WorldFromBody = worldFromBody,
            Entity = entity,
            CustomTags = tags,
            SolverType = solverType,
            Scale = scale,
        };

        var testRigidBody = new RigidBodyLayoutTest
        {
            Collider = collider,
            WorldFromBody = worldFromBody,
            Entity = entity,
            CustomTags = tags,
            SolverType = solverType,
            Scale = scale,
        };

        var bodies = new Bodies
        {
            unityBodies = new NativeArray<RigidBody>(2, Allocator.Persistent),
            testBodies = new NativeArray<RigidBodyLayoutTest>(2, Allocator.Persistent),
        };
        bodies.unityBodies[0] = unityRigidBody;
        bodies.unityBodies[1] = unityRigidBody;
        bodies.testBodies[0] = testRigidBody;
        bodies.testBodies[1] = testRigidBody;

        return bodies;
    }
}