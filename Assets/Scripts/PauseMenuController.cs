using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PBJ
{
	public class PauseMenuController : MonoBehaviour
	{
		public GameObject pauseMenuUI;

		[SerializeField]
		private string m_mainMenuSceneName;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (GameController.Instance.CurrentGameState == GameController.GameState.Paused)
				{
					Resume();
				}
				else if (GameController.Instance.CurrentGameState == GameController.GameState.Playing)
				{
					Pause();
				}
			}
		}

		public void Resume()
		{
			pauseMenuUI.SetActive(false);
			Time.timeScale = 1f;
			GameController.Instance.CurrentGameState = GameController.GameState.Playing;
		}

		void Pause()
		{
			pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			GameController.Instance.CurrentGameState = GameController.GameState.Paused;
		}

		public void LoadMenu()
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(m_mainMenuSceneName);
		}

		public void QuitGame()
		{
			Debug.Log("Quiting game...");
			Application.Quit();
		}
	}

}