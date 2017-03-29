/// <reference path="../types-gtanetwork/index.d.ts" />

//NAME,HASH,PRICE
var motorsycles = [
    ["Quad", -2128233223, 8000],
    ["Faggio", -1842748181, 5000],
    ["Hexer", -301427732, 25000],
    ["Sanchez", 788045382, 12000],
    ["PCJ", -909201658, 23000],
    ["Bagger", -2140431165, 14000]
];
var Copues = [
    ["Mini", -1177863319, 14000],
    ["Blista", 1039032026, 28000],
    ["Rhapsody", 841808271, 30000],
    ["Prairie", -1450650718, 25000]
];
var trucksnvans = [
    ["Benson", 2053223216, 60000],
    ["Mule", 904750859, 70000]
];
var offroad = [
	["Bodhi", -1435919434, 38000],
	["Sandking", -1189015600, 53000],
	["Rebel", -2045594037, 65000],
	["Mesa", 914654722, 75000],
	["RancherXL", 1645267888, 80000]
];
var musclecars = [
    ["Dominator", 80636076, 55000],
    ["Buccaneer", -682211828, 40000],
    ["Gauntlet", -1800170043, 58000],
    ["Tampa", 972671128, 34000],
    ["Ruiner", -227741703, 66000],
    ["SabreGT", -1685021548, 115000],
    ["VooDoo", 2006667053, 15000],
    ["Faction", -2119578145, 35000]
];
var suv = [
    ["Baller", -808831384, 75000],
    ["Cavalcade", 2006918058, 55000],
    ["Gresley", -1543762099, 48000],
    ["Granger", -1775728740, 70000],
    ["Dubsta", 1177543287, 95000],
    ["Huntley", 486987393, 65000],
    ["XLS", 1203490606, 39000]
];
var supercars = [
    ["Elegy", 196747873, 85000],
    ["Fusilade", 499169875, 120000],
    ["Coquette", 108773431, 150000],
    ["Lynx", 482197771, 165000]
];

//Events.
API.onServerEventTrigger.connect((eventName, args) => {
    switch(eventName) {
        case "dealership_showbuyvehiclemenu":

            break;
    }
});