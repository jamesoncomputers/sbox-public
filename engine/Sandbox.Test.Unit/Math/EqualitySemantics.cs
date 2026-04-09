namespace MathTest;

/// <summary>
/// Formalizes the equality contract for math types that use approximate == and exact Equals:
///   - operator ==  ->  AlmostEqual (approximate, for gameplay comparisons)
///   - Equals()     ->  exact bitwise (for serialization, hashing, dictionary keys)
///
/// This ensures we don't accidentally regress either direction:
///   - Making == exact would break gameplay code that relies on tolerance
///   - Making Equals approximate would break serialization round-trip checks
/// </summary>
[TestClass]
public class EqualitySemantics
{
	[TestMethod]
	public void Vector3_OperatorEquals_IsApproximate()
	{
		var a = new Vector3( 1, 2, 3 );
		var b = new Vector3( 1, 2, 3 + 5e-5f );

		Assert.IsTrue( a == b, "operator == should use AlmostEqual and treat tiny differences as equal" );
		Assert.IsFalse( a != b );
	}

	[TestMethod]
	public void Vector3_Equals_IsExact()
	{
		var a = new Vector3( 1, 2, 3 );
		var b = new Vector3( 1, 2, 3 + 5e-5f );

		Assert.IsFalse( a.Equals( b ), "Equals should be exact bitwise and reject any difference" );
	}

	[TestMethod]
	public void Vector3_Equals_IdenticalValues()
	{
		var a = new Vector3( 1, 2, 3 );
		var b = new Vector3( 1, 2, 3 );

		Assert.IsTrue( a == b );
		Assert.IsTrue( a.Equals( b ) );
	}

	[TestMethod]
	public void Rotation_OperatorEquals_IsApproximate()
	{
		var a = Rotation.Identity;
		var b = Rotation.Identity;

		// Nudge one quaternion component by a tiny amount within tolerance
		b._quat.X += 1e-5f;

		Assert.IsTrue( a == b, "operator == should use AlmostEqual and treat tiny differences as equal" );
		Assert.IsFalse( a != b );
	}

	[TestMethod]
	public void Rotation_Equals_IsExact()
	{
		var a = Rotation.Identity;
		var b = Rotation.Identity;

		b._quat.X += 1e-5f;

		Assert.IsFalse( a.Equals( b ), "Equals should be exact bitwise and reject any difference" );
	}

	[TestMethod]
	public void Rotation_Equals_IdenticalValues()
	{
		var a = Rotation.FromAxis( Vector3.Up, 45 );
		var b = Rotation.FromAxis( Vector3.Up, 45 );

		Assert.IsTrue( a == b );
		Assert.IsTrue( a.Equals( b ) );
	}

	[TestMethod]
	public void Transform_OperatorEquals_IsApproximate()
	{
		var a = new Transform( new Vector3( 100, 200, 300 ), Rotation.Identity, 1 );
		var b = new Transform( new Vector3( 100, 200, 300 + 5e-5f ), Rotation.Identity, 1 );

		Assert.IsTrue( a == b, "operator == should use AlmostEqual and treat tiny differences as equal" );
		Assert.IsFalse( a != b );
	}

	[TestMethod]
	public void Transform_Equals_IsExact()
	{
		var a = new Transform( new Vector3( 100, 200, 300 ), Rotation.Identity, 1 );
		var b = new Transform( new Vector3( 100, 200, 300 + 5e-5f ), Rotation.Identity, 1 );

		Assert.IsFalse( a.Equals( b ), "Equals should be exact bitwise and reject any difference" );
	}

	[TestMethod]
	public void Transform_Equals_IdenticalValues()
	{
		var a = new Transform( new Vector3( 100, 200, 300 ), Rotation.FromAxis( Vector3.Up, 90 ), 2 );
		var b = new Transform( new Vector3( 100, 200, 300 ), Rotation.FromAxis( Vector3.Up, 90 ), 2 );

		Assert.IsTrue( a == b );
		Assert.IsTrue( a.Equals( b ) );
	}

	/// <summary>
	/// The exact scenario that caused phantom prefab overrides: a value below
	/// the 0.0001 AlmostEqual tolerance must be distinguishable via Equals.
	/// </summary>
	[TestMethod]
	public void Transform_Equals_DetectsSubToleranceDrift()
	{
		var prefab = new Transform( new Vector3( 226, -4446, -7.247925E-05f ), Rotation.Identity, 1 );
		var instance = new Transform( new Vector3( 226, -4446, 0 ), Rotation.Identity, 1 );

		Assert.IsTrue( prefab == instance, "operator == should consider these approximately equal" );
		Assert.IsFalse( prefab.Equals( instance ), "Equals must detect the sub-tolerance difference" );
	}
}
