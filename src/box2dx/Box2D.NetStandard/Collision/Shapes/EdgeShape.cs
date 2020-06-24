﻿/*
  Box2D.NetStandard Copyright © 2020 Ben Ukhanov & Hugh Phoenix-Hulme https://github.com/benzuk/box2d-netstandard
  Box2DX Copyright (c) 2009 Ihar Kalasouski http://code.google.com/p/box2dx
  
// MIT License

// Copyright (c) 2019 Erin Catto

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
*/

using System.Numerics;
using System.Runtime.CompilerServices;
using Box2D.NetStandard.Common;
using Math = Box2D.NetStandard.Common.Math;

namespace Box2D.NetStandard.Collision.Shapes
{
	public class EdgeShape : Shape {
    internal Vector2 m_vertex1;
    internal Vector2 m_vertex2;

    internal Vector2? m_vertex0;
    internal Vector2? m_vertex3;


    public EdgeShape() {
      m_type = ShapeType.Edge;
      m_radius = Settings.PolygonRadius;
    }

    public EdgeShape(Vector2 v1, Vector2 v2):this() {
      Set(v1, v2);
    }

    public Vector2 Vertex1 {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get=>m_vertex1;
    }
    
    public Vector2 Vertex2 {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get=>m_vertex2;
    }

    public void Set(in Vector2 v1, in Vector2 v2) {
      m_vertex1    = v1;
      m_vertex2    = v2;
    }


    public override Shape Clone() {
      return (EdgeShape) MemberwiseClone();
    }

    public override int GetChildCount() => 1;

    public override bool TestPoint(in Transform xf, in Vector2 p) => false;

    public override bool RayCast(out RayCastOutput output, in RayCastInput input, in Transform xf, int childIndex) {
      output = default;
      // Put the ray into the edge's frame of reference.
      Vector2 p1 = Math.MulT(xf.q, input.p1 - xf.p);
      Vector2 p2 = Math.MulT(xf.q, input.p2 - xf.p);
      Vector2 d  = p2 - p1;

      Vector2 v1 = m_vertex1;
      Vector2 v2 = m_vertex2;
      Vector2 e  = v2 - v1;
      Vector2 normal = Vector2.Normalize(new Vector2(e.Y, -e.X));

      // q = p1 + t * d
      // dot(normal, q - v1) = 0
      // dot(normal, p1 - v1) + t * dot(normal, d) = 0
      float numerator   = Vector2.Dot(normal, v1 - p1);
      float denominator = Vector2.Dot(normal, d);

      if (denominator == 0.0f)
      {
        return false;
      }

      float t = numerator / denominator;
      if (t < 0.0f || input.maxFraction < t)
      {
        return false;
      }

      Vector2 q = p1 + t * d;

      // q = v1 + s * r
      // s = dot(q - v1, r) / dot(r, r)
      Vector2 r  = v2 - v1;
      float  rr = Vector2.Dot(r, r);
      if (rr == 0.0f)
      {
        return false;
      }

      float s = Vector2.Dot(q - v1, r) / rr;
      if (s < 0.0f || 1.0f < s)
      {
        return false;
      }

      output.fraction = t;
      if (numerator > 0.0f)
      {
        output.normal = -Math.Mul(xf.q, normal);
      }
      else
      {
        output.normal = Math.Mul(xf.q, normal);
      }
      return true;
    }

    public override void ComputeAABB(out AABB aabb, in Transform xf, int childIndex) {
      Vector2 v1 = Math.Mul(xf, m_vertex1);
      Vector2 v2 = Math.Mul(xf, m_vertex2);

      Vector2 lower = Vector2.Min(v1, v2);
      Vector2 upper = Vector2.Max(v1, v2);

      Vector2 r = new Vector2(m_radius, m_radius);
      aabb.lowerBound = lower - r;
      aabb.upperBound = upper + r;
    }

    public override void ComputeMass(out MassData massData, float density) {
      massData = new MassData(0f, 0.5f * (m_vertex1 + m_vertex2),0f);
    }

    public void Set(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3) {
      m_vertex0 = v0;
      m_vertex1 = v1;
      m_vertex2 = v2;
      m_vertex3 = v3;
    }
  }
}
