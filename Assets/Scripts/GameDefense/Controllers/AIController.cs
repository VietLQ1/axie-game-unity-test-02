using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class AIController : MonoBehaviour
    {
        DefenseState _defenseState;
        int _shootAngle = 45;
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
        }
        // Update is called once per frame
        void Update()
        {
            if (_defenseState == null || !_defenseState.isPlaying || _defenseState.playerState.shootDelay > 0) return;

            EnemyState nearestEnemy = FindTarget();
            if (nearestEnemy == null) return;
            
            int nearestPower = DeterminePower(nearestEnemy);
            if (nearestPower != -1)
            {
                if (_defenseState.energy >= DefenseState.ENERGY_SHOT_MAX_CHARGE && _defenseState.enemyStates.Count >= 3 && nearestEnemy.pos.x <= 18f)
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

        int DeterminePower(EnemyState target)
        {
            float L = Mathf.Abs(target.pos.x - _playerpos.x);
            float h = -Mathf.Abs(target.pos.y - _playerpos.y);
            float s = target.speed;
            PowerSolver _pow = new PowerSolver(L, h, s, _shootAngle);
            float _speed0 = _pow.DetermineSpeed();
            if(_speed0 < 4f)
            {
                _power = 100;
                return _power;
            }
            _power = (int)((_speed0 - DefenseState.POWER_MIN)*100f / DefenseState.POWER_BOOST_MAX);
            return _power;
        }
        public class PowerSolver
        {
            private int _shootAngle;
            private float L;
            private float h;
            private float s;
            public PowerSolver(float l, float h, float s, int shootAngle)
            {
                L = l;
                this.h = h;
                this.s = s;
                _shootAngle = shootAngle;
            }
            public float DetermineSpeed()
            {
                float _speed0 = 0f;
                float _tri = Mathf.Cos(_shootAngle * Mathf.Deg2Rad);
                float term1 = L * _tri * _tri - h * _tri * _tri - (DefenseState.GRAVITY / 2) * DefenseState.FIXED_TIME_STEP * L * _tri;
                float term2 = s * L * _tri - 2 * h * s * _tri - (DefenseState.GRAVITY / 2) * L * L - (DefenseState.GRAVITY / 2) * DefenseState.FIXED_TIME_STEP * L * s;
                float term3 = -h * s * s;
                _speed0 = QuadSolve(term1, term2, term3);
                return _speed0;
            }
            float QuadSolve(float a, float b, float c)
            {
                if (a == 0)
                {
                    if (b == 0) { return 0f; }
                    return -c / b;
                }
                float sqrtpart = (b * b) - (4 * a * c);
                if (sqrtpart < 0)
                {
                    return 0f;
                }
                float r1 = ((-1f) * b + Mathf.Sqrt(sqrtpart)) / (2f * a);
                float r2 = ((-1f) * b - Mathf.Sqrt(sqrtpart)) / (2f * a);
                if (r1 <= 40 && r1 >= 4)
                {
                    return r1;
                }
                else if (r2 <= 40 && r2 >= 4)
                {
                    return r2;
                }
                return 0f;
            }
        }
    }
}
