using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
	public class AnimationHelper : MonoBehaviour {
		
		public float valueFrom = 0.0f;
		public float valueTo = 1.0f;

		public float time = 0.0f;
		public float delay = 0.0f;

		public iTween.LoopType loopType = iTween.LoopType.none;
		public iTween.EaseType easeType = iTween.EaseType.linear;

		public bool ignoreTimescale = false;

		private bool autoPlay = true;

		void Awake() {
			if ( this.autoPlay )
				this.Play();
		}

        public virtual void Play() {
			Hashtable ht = new Hashtable();

			ht.Add( "from", this.valueFrom );
			ht.Add( "to", this.valueTo );
			ht.Add( "time", this.time );
			ht.Add( "delay", this.delay );

			ht.Add( "looptype", this.loopType );
			ht.Add( "easetype", this.easeType );

			ht.Add( "onstart", (Action<object>)( newVal => {
                OnUpdate( this.valueFrom );
                OnStart();
			} ) );
			ht.Add( "onupdate", (Action<object>)( newVal => {
                OnUpdate( (float)newVal );
			} ) );
			ht.Add( "oncomplete", (Action<object>)( newVal => {
                OnEnd();
			} ) );

			ht.Add( "ignoretimescale", ignoreTimescale );

			iTween.ValueTo( this.gameObject, ht );
		}

		public virtual void OnStart () {
			
		}

		public virtual void OnEnd () {
			
		}

		public virtual void OnUpdate (float value) {
		
		}
	}
}