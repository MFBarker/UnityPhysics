using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicCharacter : MonoBehaviour
{
	[SerializeField] float speed = 5;
	[SerializeField] float turnRate = 180;
	[SerializeField] bool isRelative = false;

	// Update is called once per frame
	void Update()
	{
		// translation
		Vector3 direction = Vector3.zero;
		direction.z = Input.GetAxis("Vertical");
		// <translate>

		// rotation
		float yaw = Input.GetAxis("Horizontal");
		// <rotate>


		// draw axis
		Debug.DrawRay(transform.position, transform.forward * 2, Color.blue);
		Debug.DrawRay(transform.position, transform.right * 2, Color.red);
		Debug.DrawRay(transform.position, transform.up * 2, Color.green);
	}
}
