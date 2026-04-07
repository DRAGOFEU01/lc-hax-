using System;
using System.Threading.Tasks;
using UnityEngine;

[Command("autopattern")]
sealed class AutoPatternCommand : ICommand
{
    static PatternRunner? runner;

    public async Task Execute(Arguments args, System.Threading.CancellationToken cancellationToken)
    {
        if (Helper.LocalPlayer == null) return;

        string cmd = args.Length > 0 ? args[0].ToLowerInvariant() : "";
        string pattern = args.Length > 1 ? args[1].ToLowerInvariant() : "circle";
        float speed = args.Length > 2 && float.TryParse(args[2], out float sp) ? sp : 10f;

        switch (cmd)
        {
            case "on":
                if (runner == null)
                {
                    GameObject go = new GameObject("AutoPatternRunner");
                    runner = go.AddComponent<PatternRunner>();
                    runner.Init(pattern, speed);
                }
                Chat.Print($"AutoPattern activé ({pattern}) !");
                break;

            case "off":
                if (runner != null)
                {
                    runner.Stop();
                    runner = null;
                }
                Chat.Print("AutoPattern désactivé !");
                break;

            default:
                Chat.Print("Usage: /autopattern on/off [pattern=circle/square] [speed]");
                break;
        }

        await Task.Yield();
    }

    class PatternRunner : MonoBehaviour
    {
        string pattern = "circle";
        float speed = 10f;
        Vector3 center;
        float angle;
        int squareStep;
        float squareSide = 5f;

        internal void Init(string p, float s)
        {
            this.pattern = p;
            this.speed = s;
            if (Helper.LocalPlayer != null) this.center = Helper.LocalPlayer.transform.position;
        }

        internal void Stop()
        {
            Destroy(this.gameObject);
        }

        void Update()
        {
            if (Helper.LocalPlayer == null) return;

            switch (this.pattern)
            {
                case "circle":
                    this.MoveCircle();
                    break;
                case "square":
                    this.MoveSquare();
                    break;
            }
        }

        void MoveCircle()
        {
            if (Helper.LocalPlayer == null) return;
            this.angle += this.speed * Time.deltaTime;
            float rad = this.angle * Mathf.Deg2Rad;
            float radius = 5f;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            Helper.LocalPlayer.transform.position = this.center + offset + Vector3.up * 2f;
            Helper.LocalPlayer.transform.Rotate(Vector3.up * this.speed * Time.deltaTime);
        }

        void MoveSquare()
        {
            if (Helper.LocalPlayer == null) return;
            float stepSize = this.speed * Time.deltaTime;
            Vector3 target = this.center;

            switch (this.squareStep)
            {
                case 0: target = this.center + new Vector3(this.squareSide, 0, 0); break;
                case 1: target = this.center + new Vector3(this.squareSide, 0, this.squareSide); break;
                case 2: target = this.center + new Vector3(0, 0, this.squareSide); break;
                case 3: target = this.center; break;
            }

            Vector3 dir = target - Helper.LocalPlayer.transform.position;
            if (dir.magnitude < 0.1f)
            {
                this.squareStep = (this.squareStep + 1) % 4;
            }
            else
            {
                Helper.LocalPlayer.transform.position += dir.normalized * stepSize;
            }
        }
    }
}