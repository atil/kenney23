﻿using System.Collections.Generic;
using UnityEngine;
using JamKit;

namespace Game
{
    public class Footsteps : MonoBehaviour
    {
        private const float DistancePerStep = 1f;

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private AudioClip[] _stepClips;

        [SerializeField]
        private AudioClip _jumpClip;

        [SerializeField]
        private AudioClip _landClip;

        private List<AudioClip> _shuffledClips;
        private Vector3 _prevPos;
        private float _distanceCovered;
        private float _totalDistanceCovered = 0;

        public float TotalDistanceCovered => _totalDistanceCovered;

        private void Start()
        {
            _shuffledClips = new List<AudioClip>(_stepClips);
        }

        public void ExternalUpdate(bool isGonnaJump, bool isGrounded, bool isLandedThisFrame)
        {
            if (_distanceCovered > DistancePerStep)
            {
                _distanceCovered = 0;

                _audioSource.PlayOneShot(_shuffledClips[0]);
                _shuffledClips.Shuffle();
            }

            // Kenney: don't need to jump
            //if (isGonnaJump && isGrounded)
            //{
            //    _audioSource.PlayOneShot(_jumpClip);
            //}

            //if (isLandedThisFrame && !isGonnaJump)
            //{
            //    _audioSource.PlayOneShot(_landClip);
            //}

            if (isGrounded)
            {
                float delta = Vector3.Distance(_prevPos.WithY(0), transform.position.WithY(0));
                _distanceCovered += delta;
                _totalDistanceCovered += delta;
            }
            _prevPos = transform.position;
        }
    }
}
