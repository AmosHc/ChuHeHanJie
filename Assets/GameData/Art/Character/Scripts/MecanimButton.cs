using UnityEngine;
using System.Collections;

public class MecanimButton : MonoBehaviour
{
	public Animator[] animator;
	public string int_name;
	public int event_value;

	void OnMouseDown ()
	{
		Debug.Log ("Triggered : " + int_name);
		//animator.SetTrigger(trigger_name);
		foreach(Animator a in animator){
			a.SetInteger(int_name,event_value);
		}
		//animator.SetInteger(int_name,event_value);
	}
	

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (transform.position, 0.1f);
		if (GetComponent<Collider2D> () != null)
			Gizmos.DrawWireCube (transform.position, GetComponent<Collider2D> ().bounds.size);
	}
}
