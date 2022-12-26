using System.Collections.Generic;

namespace AudioBox.ASF
{
	public enum ASFClipState
	{
		ExitMin,
		ExitMax,
		Update,
		EnterLeft,
		EnterRight,
	}

	public abstract class ASFClip
	{
		public double       MinTime { get; set; }
		public double       MaxTime { get; set; }
		public double       Length  => MaxTime - MinTime;
		public ASFClipState State   { get; private set; }

		protected ASFClip(double _MinTime, double _MaxTime)
		{
			MinTime = _MinTime;
			MaxTime = _MaxTime;
			State   = ASFClipState.ExitMin;
		}

		public void Sample(double _Time)
		{
			ProcessEnter(_Time);
			
			ProcessUpdate(_Time);
			
			ProcessExit(_Time);
		}

		void ProcessEnter(double _Time)
		{
			if (_Time >= MinTime && State == ASFClipState.ExitMin)
			{
				State = ASFClipState.EnterLeft;
				OnEnterMin(_Time);
			}
			else if (_Time <= MaxTime && State == ASFClipState.ExitMax)
			{
				State = ASFClipState.EnterRight;
				OnEnterMax(_Time);
			}
		}

		public abstract ASFClip Clone();

		void ProcessUpdate(double _Time)
		{
			if (_Time >= MinTime && State == ASFClipState.EnterLeft)
			{
				State = ASFClipState.Update;
				OnSample(_Time);
			}
			else if (_Time >= MinTime && _Time <= MaxTime && State == ASFClipState.Update)
			{
				OnSample(_Time);
			}
			else if (_Time <= MaxTime && State == ASFClipState.EnterRight)
			{
				State = ASFClipState.Update;
				OnSample(_Time);
			}
		}

		void ProcessExit(double _Time)
		{
			if (_Time < MinTime && State == ASFClipState.Update)
			{
				State = ASFClipState.ExitMin;
				OnExitMin(_Time);
			}
			else if (_Time > MaxTime && State == ASFClipState.Update)
			{
				State = ASFClipState.ExitMax;
				OnExitMax(_Time);
			}
		}

		protected virtual void OnEnterMin(double _Time) { }

		protected virtual void OnEnterMax(double _Time) { }

		protected virtual void OnSample(double _Time) { }

		protected virtual void OnExitMin(double _Time) { }

		protected virtual void OnExitMax(double _Time) { }
	}
}
