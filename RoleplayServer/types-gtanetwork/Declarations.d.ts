/// <reference path="index.d.ts" />

declare const API: GTANetwork.Javascript.ScriptContext;
import Keys = System.Windows.Forms.Keys;
import Point = System.Drawing.Point;
import PointF = System.Drawing.PointF;
import Size = System.Drawing.Size;
import LocalHandle = GTANetwork.Util.LocalHandle;
import menuControl = NativeUI.UIMenu_MenuControls;

declare var resource: any;

declare interface IConnectedEvent {
    disconnect(): void;
}

declare interface IEvent<THandler> {
    connect(handler: THandler): IConnectedEvent;
}

declare module Enums {
    export const enum Controls {}
}