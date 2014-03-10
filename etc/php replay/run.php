<?php
include "mpqfile.php";
include "sc2replayutils.php";
include "sc2replay.php";

$mpq = new MPQFile("replay.SC2Replay", TRUE, 0);
$replay = $mpq->parseReplay();
$players = $replay->getPlayers();
$recorder = $replay->getRecorder();
$events = $replay->getEvents();
var_dump($events);