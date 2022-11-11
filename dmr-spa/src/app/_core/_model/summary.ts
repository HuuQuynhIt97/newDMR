export interface IIngredient {
    id: number;
    scanStatus: boolean;
    code: string;
    scanCode: string;
    materialNO: string;
    partNO: string;
    name: string;
    percentage: string;
    position: string;
    allow: number;
    expected: any;
    real: number;
    focusReal: boolean;
    focusExpected: boolean;
    valid: boolean;
    info: string;
    batch: string;
    unit: string;
    time_start: Date; // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)

}
