﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using PBJ.Configuration.Input;

namespace PBJ
{
	public class TwoAxisMovement : MonoBehaviour
	{
		private float m_currentSpeed;
		private Player m_input;
		private Vector2 m_currentInput;

		private Rigidbody2D m_rigid;
		private PlayerStatus m_status;
		private void Awake()
		{
			m_input = ReInput.players.GetPlayer(0);
			m_input.AddInputEventDelegate(UpdateAxis, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, Actions.HorizontalMove);
			m_input.AddInputEventDelegate(UpdateAxis, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, Actions.VerticalMove);

			if (!TryGetComponent<Rigidbody2D>(out m_rigid))
			{
				Debug.LogError("No rigidbody assigned");
			}
			if (!TryGetComponent<PlayerStatus>(out m_status))
			{
				Debug.LogError("No status assigned");
			}
		}

		private void FixedUpdate()
		{
			UpdateMovement();
		}

		private void UpdateAxis(InputActionEventData data)
		{
			switch (data.actionId)
			{
				case Actions.HorizontalMove:
					m_currentInput.x = data.GetAxisRaw();
					break;
				case Actions.VerticalMove:
					m_currentInput.y = data.GetAxisRaw();
					break;
			}
            if(m_status.CanAct)
            {
                m_status.SetFacing(m_currentInput.normalized);
            }
		}

		private void UpdateMovement()
		{
			if (m_status.CanAct)
			{
				if (m_currentInput == Vector2.zero)
				{
					m_currentSpeed = 0;
				}
				else
				{
					m_currentSpeed = m_status.MaxSpeed;
				}
				Vector2 newPosition = (Vector2)transform.position + (m_currentInput.normalized * m_currentSpeed * Time.deltaTime);
				m_rigid.MovePosition(newPosition);
			}
		}
	}
}
