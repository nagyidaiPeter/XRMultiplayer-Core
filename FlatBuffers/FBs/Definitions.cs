// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace hololensMulti
{

using global::System;
using global::FlatBuffers;

public struct Vec3 : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public Vec3 __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float X { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
  public float Y { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }
  public float Z { get { return __p.bb.GetFloat(__p.bb_pos + 8); } }

  public static Offset<hololensMulti.Vec3> CreateVec3(FlatBufferBuilder builder, float X, float Y, float Z) {
    builder.Prep(4, 12);
    builder.PutFloat(Z);
    builder.PutFloat(Y);
    builder.PutFloat(X);
    return new Offset<hololensMulti.Vec3>(builder.Offset);
  }
};

public struct Quat : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public Quat __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float X { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
  public float Y { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }
  public float Z { get { return __p.bb.GetFloat(__p.bb_pos + 8); } }
  public float W { get { return __p.bb.GetFloat(__p.bb_pos + 12); } }

  public static Offset<hololensMulti.Quat> CreateQuat(FlatBufferBuilder builder, float X, float Y, float Z, float W) {
    builder.Prep(4, 16);
    builder.PutFloat(W);
    builder.PutFloat(Z);
    builder.PutFloat(Y);
    builder.PutFloat(X);
    return new Offset<hololensMulti.Quat>(builder.Offset);
  }
};

public struct HandState : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public HandState __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public byte Pinky { get { return __p.bb.Get(__p.bb_pos + 0); } }
  public byte Ring { get { return __p.bb.Get(__p.bb_pos + 1); } }
  public byte Middle { get { return __p.bb.Get(__p.bb_pos + 2); } }
  public byte Index { get { return __p.bb.Get(__p.bb_pos + 3); } }
  public byte Thumb { get { return __p.bb.Get(__p.bb_pos + 4); } }

  public static Offset<hololensMulti.HandState> CreateHandState(FlatBufferBuilder builder, byte Pinky, byte Ring, byte Middle, byte Index, byte Thumb) {
    builder.Prep(1, 5);
    builder.PutByte(Thumb);
    builder.PutByte(Index);
    builder.PutByte(Middle);
    builder.PutByte(Ring);
    builder.PutByte(Pinky);
    return new Offset<hololensMulti.HandState>(builder.Offset);
  }
};


}
