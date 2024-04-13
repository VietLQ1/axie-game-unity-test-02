using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class AIController : MonoBehaviour
    {
        DefenseState _defenseState;
        int _shootAngle = 0;
        int _power = 100;
        AxieObject Player;
        Vector2 _playerpos;
        private void Awake()
        {
            var manager = FindObjectOfType<GameDefenseManager>();
            if (manager == null)
            {
                this.enabled = false;
            }
            else
            {
                _defenseState = manager.GetState();
            }
        }
        private void Start()
        {
            Player = FindObjectOfType<AxieObject>();
            _playerpos = Player.transform.localPosition;
            //Debug.Log(_playerpos);
        }
        // Update is called once per frame
        void Update()
        {
            if (_defenseState == null || !_defenseState.isPlaying || _defenseState.playerState.shootDelay > 0) return;

            EnemyState nearestEnemy = FindTarget();
            if (nearestEnemy == null) return;
            //Debug.Log(nearestEnemy.pos);
            Vector2 _aimline = nearestEnemy.pos - _playerpos;
            _shootAngle = -(int)Vector2.Angle(_aimline, Vector2.right) + (int)nearestEnemy.pos.x + 10*math.max(((int)nearestEnemy.pos.x/10),1);
            //Debug.Log(nearestEnemy.pos.x); 
            int nearestPower = DeterminePower(nearestEnemy.pos);
            if (nearestPower != -1)
            {
                if (_defenseState.energy >= DefenseState.ENERGY_SHOT_MAX_CHARGE)
                {
                    _defenseState.DoShootSpecial(_shootAngle, nearestPower);
                }
                else
                {
                    _defenseState.DoShoot(_shootAngle, nearestPower);
                }
            }
        }

        EnemyState FindTarget()
        {
            EnemyState nearestEnemy = null;
            float nearestDist = -1;
            foreach (var p in _defenseState.enemyStates)
            {
                var enemy = p.Value;
                if (nearestEnemy == null || enemy.pos.x < nearestDist)
                {
                    nearestDist = enemy.pos.x;
                    nearestEnemy = enemy;
                }
            }
            return nearestEnemy;
        }

        int DeterminePower(Vector2 target)
        {
            _power += 10;
            if(_power > 100)
            {
                _power = 100;
            }
            return _power;
        }
    }
}
