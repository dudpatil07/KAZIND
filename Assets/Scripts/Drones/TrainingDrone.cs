using UnityEngine;

public class TrainingDrone : MonoBehaviour
{
    public enum DroneClass
    {
        Cheap,
        Military
    }

    public enum DroneVariant
    {
        CheapScout,
        CheapRacer,
        CheapHeavy,
        MilitarySurveillance,
        MilitaryHeavy,
        MilitaryInterceptor
    }

    public enum AIState
    {
        Patrol,
        Approaching,
        Surrounding
    }

    [Header("Identity")]
    public DroneClass droneClass;
    public DroneVariant variant;
    public string displayName;

    [Header("AI")]
    public AIState aiState = AIState.Patrol;

    public float patrolRadius = 18f;
    public float patrolHeight = 20f;

    public float approachSpeed = 8f;
    public float surroundSpeed = 7f;

    public float surroundDistance = 20f;
    public float surroundHeight = 18f;

    [Header("Activation")]
    public float activationDistance = 45f;

    [Header("Flight Safety")]
    public float minimumAltitude = 8f;
    public float maximumAltitude = 55f;

    private Transform player;

    private Vector3 patrolCenter;
    private Vector3 lastValidPosition;

    private float patrolTime;
    private float phase;

    private int surroundSlot;

    private Transform[] rotors;

    // ============================================================
    // CONFIGURATION
    // ============================================================

    public void Configure(
        DroneClass newClass,
        DroneVariant newVariant,
        string newName,
        Vector3 center,
        float newAltitude,
        float newRadius,
        float newSpeed,
        float newPhase
    )
    {
        droneClass = newClass;
        variant = newVariant;
        displayName = newName;

        patrolCenter = center;

        patrolHeight =
            Mathf.Max(
                minimumAltitude,
                newAltitude
            );

        patrolRadius =
            Mathf.Max(
                8f,
                newRadius
            );

        approachSpeed =
            Mathf.Max(
                4f,
                newSpeed
            );

        phase =
            newPhase;

        patrolTime = 0f;

        lastValidPosition =
            center;

        CacheRotors();
    }

    public void ConfigureAI(
        Transform targetPlayer,
        int slot
    )
    {
        player =
            targetPlayer;

        surroundSlot =
            slot;

        // IMPORTANT:
        // Whatever position the drone is currently in
        // becomes its new patrol center.
        patrolCenter =
            transform.position;

        // Never allow spawn below safe altitude.
        if (
            patrolCenter.y <
            minimumAltitude
        )
        {
            patrolCenter.y =
                minimumAltitude;

            transform.position =
                patrolCenter;
        }

        patrolHeight =
            patrolCenter.y;

        lastValidPosition =
            transform.position;
    }

    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        CacheRotors();

        if (player == null)
        {
            GameObject p =
                GameObject.Find(
                    "TraineePlayer"
                );

            if (p != null)
                player =
                    p.transform;
        }

        patrolCenter =
            transform.position;

        if (
            patrolCenter.y <
            minimumAltitude
        )
        {
            patrolCenter.y =
                minimumAltitude;

            transform.position =
                patrolCenter;
        }

        patrolHeight =
            patrolCenter.y;

        lastValidPosition =
            transform.position;
    }

    void CacheRotors()
    {
        Transform[] all =
            GetComponentsInChildren<Transform>();

        System.Collections.Generic.List<Transform> list =
            new System.Collections.Generic.List<Transform>();

        foreach (Transform t in all)
        {
            if (t.name.Contains("Rotor"))
                list.Add(t);
        }

        rotors =
            list.ToArray();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    void Update()
    {
        if (player == null)
        {
            GameObject p =
                GameObject.Find(
                    "TraineePlayer"
                );

            if (p != null)
                player =
                    p.transform;
        }

        UpdateAI();
        AnimateRotors();
        EnforceAirspace();
    }

    // ============================================================
    // AI STATE
    // ============================================================

    void UpdateAI()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float distance =
            Vector3.Distance(
                new Vector3(
                    player.position.x,
                    0f,
                    player.position.z
                ),
                new Vector3(
                    patrolCenter.x,
                    0f,
                    patrolCenter.z
                )
            );

        // Player has left the protected/base area.
        if (
            distance >
            activationDistance
        )
        {
            if (
                aiState ==
                AIState.Patrol
            )
            {
                aiState =
                    AIState.Approaching;
            }
        }

        switch (aiState)
        {
            case AIState.Patrol:

                Patrol();

                break;

            case AIState.Approaching:

                ApproachPlayer();

                break;

            case AIState.Surrounding:

                SurroundPlayer();

                break;
        }
    }

    // ============================================================
    // PATROL
    // ============================================================

    void Patrol()
    {
        patrolTime +=
            Time.deltaTime *
            approachSpeed *
            0.35f;

        float angle =
            phase +
            patrolTime * 0.35f;

        float x =
            Mathf.Cos(angle) *
            patrolRadius;

        float z =
            Mathf.Sin(angle) *
            patrolRadius;

        float y =
            patrolHeight +
            Mathf.Sin(
                patrolTime * 0.7f
            ) * 2f;

        y =
            Mathf.Clamp(
                y,
                minimumAltitude,
                maximumAltitude
            );

        Vector3 target =
            patrolCenter +
            new Vector3(
                x,
                y - patrolCenter.y,
                z
            );

        MoveTowards(
            target,
            approachSpeed * 0.45f
        );
    }

    // ============================================================
    // APPROACH PLAYER
    // ============================================================

    void ApproachPlayer()
    {
        Vector3 playerPos =
            player.position;

        // Approach from above the player's level,
        // never from the ground.
        float targetY =
            Mathf.Clamp(
                playerPos.y +
                surroundHeight,
                minimumAltitude,
                maximumAltitude
            );

        Vector3 target =
            new Vector3(
                playerPos.x,
                targetY,
                playerPos.z
            );

        MoveTowards(
            target,
            approachSpeed
        );

        float horizontalDistance =
            Vector2.Distance(
                new Vector2(
                    transform.position.x,
                    transform.position.z
                ),
                new Vector2(
                    player.position.x,
                    player.position.z
                )
            );

        if (
            horizontalDistance <
            surroundDistance * 1.35f
        )
        {
            aiState =
                AIState.Surrounding;
        }
    }

    // ============================================================
    // SURROUND PLAYER
    // ============================================================

    void SurroundPlayer()
    {
        if (player == null)
            return;

        // Six stable positions around player.
        float[] angles =
        {
            0f,
            60f,
            120f,
            180f,
            240f,
            300f
        };

        float angle =
            angles[
                surroundSlot %
                angles.Length
            ];

        // Small time variation so drones do not
        // look completely frozen.
        angle +=
            Mathf.Sin(
                Time.time * 0.25f +
                surroundSlot
            ) * 8f;

        float radians =
            angle *
            Mathf.Deg2Rad;

        float distance =
            surroundDistance +
            (surroundSlot % 2) * 5f;

        float height =
            surroundHeight +
            (surroundSlot % 3) * 3f;

        Vector3 target =
            player.position +
            new Vector3(
                Mathf.Cos(radians) *
                distance,

                height,

                Mathf.Sin(radians) *
                distance
            );

        target.y =
            Mathf.Clamp(
                target.y,
                minimumAltitude,
                maximumAltitude
            );

        MoveTowards(
            target,
            surroundSpeed
        );

        // Keep watching the player
        Vector3 direction =
            player.position -
            transform.position;

        if (
            direction.sqrMagnitude >
            0.01f
        )
        {
            Quaternion rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotation,
                    2f *
                    Time.deltaTime
                );
        }
    }

    // ============================================================
    // MOVEMENT
    // ============================================================

    void MoveTowards(
        Vector3 target,
        float speed
    )
    {
        Vector3 direction =
            target -
            transform.position;

        if (
            direction.sqrMagnitude >
            0.1f
        )
        {
            Quaternion rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotation,
                    3f *
                    Time.deltaTime
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target,
                speed *
                Time.deltaTime
            );
    }

    // ============================================================
    // AIRSPACE SAFETY
    // ============================================================

    void EnforceAirspace()
    {
        Vector3 p =
            transform.position;

        bool invalid =
            float.IsNaN(p.x) ||
            float.IsNaN(p.y) ||
            float.IsNaN(p.z) ||
            float.IsInfinity(p.x) ||
            float.IsInfinity(p.y) ||
            float.IsInfinity(p.z);

        if (invalid)
        {
            transform.position =
                lastValidPosition;

            return;
        }

        // Hard floor
        if (
            p.y <
            minimumAltitude
        )
        {
            p.y =
                minimumAltitude;
        }

        // Hard ceiling
        if (
            p.y >
            maximumAltitude
        )
        {
            p.y =
                maximumAltitude;
        }

        transform.position =
            p;

        lastValidPosition =
            p;
    }

    // ============================================================
    // ROTORS
    // ============================================================

    void AnimateRotors()
    {
        if (rotors == null)
            return;

        foreach (
            Transform rotor
            in rotors
        )
        {
            rotor.Rotate(
                Vector3.up,
                1800f *
                Time.deltaTime,
                Space.Self
            );
        }
    }

    // ============================================================
    // INFORMATION
    // ============================================================

    public string GetAirframeType()
    {
        switch (variant)
        {
            case DroneVariant.CheapScout:
                return "LIGHT SCOUT QUAD";

            case DroneVariant.CheapRacer:
                return "HIGH-SPEED QUAD";

            case DroneVariant.CheapHeavy:
                return "HEAVY UTILITY QUAD";

            case DroneVariant.MilitarySurveillance:
                return "SURVEILLANCE UAV";

            case DroneVariant.MilitaryHeavy:
                return "HEAVY UAV";

            case DroneVariant.MilitaryInterceptor:
                return "TACTICAL UAV";

            default:
                return "UNKNOWN";
        }
    }

    public string GetThreatLevel()
    {
        switch (variant)
        {
            case DroneVariant.CheapScout:
                return "LOW";

            case DroneVariant.CheapRacer:
                return "LOW";

            case DroneVariant.CheapHeavy:
                return "MEDIUM";

            case DroneVariant.MilitarySurveillance:
                return "HIGH";

            case DroneVariant.MilitaryHeavy:
                return "CRITICAL";

            case DroneVariant.MilitaryInterceptor:
                return "HIGH";

            default:
                return "UNKNOWN";
        }
    }

    public string GetClassText()
    {
        return
            droneClass ==
            DroneClass.Cheap
            ? "CHEAP"
            : "MILITARY";
    }
}
