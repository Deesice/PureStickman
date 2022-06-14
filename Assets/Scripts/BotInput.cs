using System.Collections;
using System.Collections.Generic;
using UnityEngine;
struct TimeStamp
{
    float time;
    bool flag;
    public void SetStamp()
    {
        if (!flag)
        {
            flag = true;
            time = Time.time;
        }
    }
    public float GetTime()
    {
        if (flag)
            return Time.time - time;
        else
            return 1000000;
    }
    public void Reset()
    {
        flag = false;
    }
}

public class BotInput : ICharacterInput
{
    Enemy nearestEnemy;
    Transform transform;
    TimeStamp tensionStart;
    TimeStamp reaction;
    TimeStamp movingToSafeDistance;
    BotProperties properties;
    CustomJoystick joystick;
    GoalZone goal;
    Vector3 GoalPosition
    {
        get
        {
            if (DifficultyManager.GetGoalType() == GoalType.Escort)
            {
                float f = -1000000;
                foreach (var n in NPC.AllNPCs)
                {
                    if (n.transform.position.z > f)
                        f = n.transform.position.z;

                }
                return new Vector3(0, 0, f);
            }
            else
            {
                if (goal)
                    return goal.transform.position;
                else
                    return transform.position;
            }
        }
    }
    Player player;
    public BotInput(Transform me, BotProperties properties)
    {
        goal = GameObject.FindObjectOfType<GoalZone>();
        transform = me;
        this.properties = properties;
        joystick = GameObject.FindObjectOfType<CustomJoystick>(true);
        player = GameObject.FindObjectOfType<Player>(true);
    }
    bool ProcessVisibleEnemy(Enemy enemy)
    {
        reaction.SetStamp();

        if (reaction.GetTime() > properties.reaction)
        {
            tensionStart.SetStamp();
            if (tensionStart.GetTime() > properties.tensionTime)
            {
                tensionStart.Reset();
                reaction.Reset();
                return false;
            }
            else
            {
                return true;
            }
        }
        
        return false;
    }
    public bool GetAimingPoint(out Vector3 point)
    {
        bool output;
        point = Vector3.zero;
        if (nearestEnemy &&
            Mathf.Abs(nearestEnemy.transform.position.z - transform.position.z) <= properties.sightDistance)
        {
            //стреляем выше головы на половину толщины капсулы стрелы (толщина = 0,15 метров)
            point = nearestEnemy.HeadUppiestPosition;// + new Vector3(0, 0.075f, 0);
            point.x = 0;
            output = ProcessVisibleEnemy(nearestEnemy);
            if (output)
            {
                Crosshair.instance.SwitchAim(true);
                Crosshair.instance.FocusOnPosition(point);
            }
        }
        else
        {
            if (tensionStart.GetTime() > properties.tensionTime)
            {
                tensionStart.Reset();
                reaction.Reset();
                output = false;
            }
            else
            {
                output = true;
            }
        }
        if (!output)
        {
            //рандомизируем точку, чтобы сымитировать промах
            var diff = Vector3.Cross(point - Player.ArrowSpawnPosition, Vector3.right).normalized;
            point += diff * Random.Range(-properties.targetingError, properties.targetingError);
        }
        return output;
    }
    public float GetMovementInput()
    {
        if (Crosshair.instance.AimingNow && !properties.canMoveWhileAiming)
            return 0;

        float distanceToEnemy = 0;
        var distanceToGoal = GoalPosition.z - transform.position.z;
        if (nearestEnemy)
            distanceToEnemy = nearestEnemy.transform.position.z - transform.position.z;
        else
            return Mathf.Clamp(distanceToGoal, -1, 1);

        if (Mathf.Abs(distanceToGoal) > 0 && NPC.AllNPCs.Count > 0)
        {
            if (Mathf.Abs(distanceToEnemy) < properties.safeDistance)
            {
                movingToSafeDistance.SetStamp();
            }

            if (movingToSafeDistance.GetTime() <= properties.movingSafeTime)
            {
                return -Mathf.Sign(distanceToEnemy);
            }
            else
            {
                movingToSafeDistance.Reset();
                return Mathf.Clamp(distanceToGoal, -1, 1);
            }
        }
        else
        {
            if (Mathf.Abs(distanceToEnemy) > properties.sightDistance)
            {
                movingToSafeDistance.Reset();
                return Mathf.Sign(distanceToEnemy);
            }
            else if (Mathf.Abs(distanceToEnemy) < properties.safeDistance)
            {
                movingToSafeDistance.SetStamp();
            }

            if (movingToSafeDistance.GetTime() <= properties.movingSafeTime)
            {
                return -Mathf.Sign(distanceToEnemy);
            }
            else
            {
                movingToSafeDistance.Reset();
                if (reaction.GetTime() <= properties.reaction)
                    return 0;
                else
                    return Mathf.Clamp(distanceToGoal, -1, 1);
            }
        }
    }

    public void PreUpdate()
    {
        nearestEnemy = EnemyRegistrator.GetNearestEnemy(1, transform, Predicate);
    }
    bool Predicate(Enemy e)
    {
        return !e.IsImmortal;
    }
}
