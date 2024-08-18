import taskingRoutes from "./taskingRoutes.js";
import { Router } from "express";

const router = Router();

router.use("/tasking", taskingRoutes);

export default router;