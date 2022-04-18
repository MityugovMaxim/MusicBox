using System;
using UnityEngine;

namespace AudioBox.ASF
{
	public static class ASFMath
	{
		public static double Clamp(double _Value, double _Min, double _Max)
		{
			return _Value < _Min ? _Min : _Value > _Max ? _Max : _Value;
		}

		public static double Clamp01(double _Value)
		{
			return _Value < 0d ? 0d : _Value > 1 ? 1d : _Value;
		}

		public static double Lerp(double _A, double _B, double _Time)
		{
			return _A + (_B - _A) * Clamp01(_Time);
		}

		public static double LerpUnclamped(double _A, double _B, double _Time)
		{
			return _A + (_B - _A) * _Time;
		}

		public static double Remap(double _Value, double _A, double _B)
		{
			return Math.Abs(_A - _B) > double.Epsilon * 2 ? (_Value - _A) / (_B - _A) : 0d;
		}

		public static double RemapClamped(double _Value, double _A, double _B)
		{
			return Clamp(Remap(_Value, _A, _B), 0d, 1d);
		}

		public static double Remap(double _Value, double _A0, double _B0, double _A1, double _B1)
		{
			return Math.Abs(_A0 - _B0) > double.Epsilon * 2 ? _A1 + (_Value - _A0) * (_B1 - _A1) / (_B0 - _A0) : 0d;
		}

		public static double RemapClamped(double _Value, double _A0, double _B0, double _A1, double _B1)
		{
			return Clamp(Remap(_Value, _A0, _B0, _A1, _B1), _A1, _B1);
		}

		public static float TimeToPosition(double _Time, double _MinTime, double _MaxTime, float _MinPosition, float _MaxPosition)
		{
			double phase = ASFMath.Remap(_Time, _MinTime, _MaxTime);
			
			return (float)ASFMath.LerpUnclamped(_MinPosition, _MaxPosition, phase);
		}

		public static double PositionToTime(float _Position, float _MinPosition, float _MaxPosition, double _MinTime, double _MaxTime)
		{
			double phase = ASFMath.Remap(_Position, _MinPosition, _MaxPosition);
			
			return ASFMath.LerpUnclamped(_MinTime, _MaxTime, phase);
		}

		public static float TimeToPhase(double _Time, double _MinTime, double _MaxTime)
		{
			return (float)ASFMath.Remap(_Time, _MinTime, _MaxTime);
		}

		public static float PhaseToPosition(float _Phase, float _MinPosition, float _MaxPosition)
		{
			return (float)Remap(_Phase, 0, 1, _MinPosition, _MaxPosition);
		}

		public static float PositionToPhase(float _Position, float _MinPosition, float _MaxPosition)
		{
			return (float)Remap(_Position, _MinPosition, _MaxPosition);
		}

		public static double SnapTime(double _Time, double _Step)
		{
			return Math.Abs(_Step) > double.Epsilon * 2 ? Math.Round(_Time / _Step) * _Step : 0d;
		}

		public static float SnapPhase(float _Phase, float _Step)
		{
			return Mathf.Abs(_Phase) > float.Epsilon * 2 ? Mathf.Round(_Phase / _Step) * _Step : 0f;
		}
	}
}