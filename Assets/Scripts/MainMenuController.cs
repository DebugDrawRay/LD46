using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PBJ
{
	public class MainMenuController : MonoBehaviour
	{
		public string m_gameSceneString;

		public void PlayGame()
		{
			Debug.Log("Playing game...");
			SceneManager.LoadScene(m_gameSceneString);
		}

		public void QuitGame()
		{
			Debug.Log("Quiting game...");
			Application.Quit();
		}
	}
}
