// Fades out the Text of the HPParticle and Destroys it 

using UnityEngine;
using System.Collections;

public class HPParticleScript : MonoBehaviour {

	public float Alpha =1f;
	public float FadeSpeed = 0.5f;

	private GameObject HPLabel;
	private GameObject sprite;
	// Set a Variable
	void Start () 
	{
		HPLabel = gameObject.transform.Find("HPLabel").gameObject;
		sprite = gameObject.transform.Find("Sprite").gameObject;
	}

	void FixedUpdate () 
	{
		Alpha = Mathf.Lerp(Alpha,0f,FadeSpeed * Time.deltaTime);

		Color CurrentColor = HPLabel.GetComponent<TextMesh>().color;
		Color spriteColor = sprite.GetComponent<SpriteRenderer>().color;
		HPLabel.GetComponent<TextMesh>().color = new Color(CurrentColor.r,CurrentColor.g,CurrentColor.b,Alpha);
		sprite.GetComponent<SpriteRenderer>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, Alpha);

		if (Alpha < 0.005f)
		{
			Destroy(gameObject);
		}
	}
}
