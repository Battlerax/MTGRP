/// <reference path="index.d.ts" />

declare const API: GTANetwork.Javascript.ScriptContext;
import Keys = System.Windows.Forms.Keys;
import Point = System.Drawing.Point;
import PointF = System.Drawing.PointF;
import Size = System.Drawing.Size;
import LocalHandle = GTANetwork.Util.LocalHandle;

declare var resource: any;

declare interface IEvent<THandler> {
    connect(handler: THandler): void;
}

declare module Enums {
    export const enum Controls {}
}